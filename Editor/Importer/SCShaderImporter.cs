using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace jp.lilxyzw.shadercore
{
    [ScriptedImporter(1, "scshader")]
    internal partial class SCShaderImporter : ScriptedImporter
    {
        private static readonly Regex REG_INCLUDE = new(@"^\s*#include\s*""([^""]*)""\s*(//|$)");
        private static readonly Regex REG_PHASE = new(@"^\s*__SC_PHASE_([a-zA-Z0-9_]+)__\s*$");
        private static readonly Regex REG_INCLUDES = new(@"^\s*__SC_INCLUDES__\s*$");
        private static readonly Regex REG_INDENT = new(@"^\s*");
        private readonly static Regex REG_SCConstValue = new(@"\[SCConstValue\s*\((.*)\)\]");
        private readonly static Regex REG_HLSLINCLUDE = new(@"^\s*HLSLINCLUDE");
        private readonly static Regex REG_HLSLPROGRAM = new(@"^\s*HLSLPROGRAM");
        private readonly static Regex REG_ENDHLSL = new(@"^\s*ENDHLSL");

        private AssetImportContext ctx;
        private readonly List<SCModule> modules = new();
        private readonly List<SCPhase> phases = new();
        private readonly StringBuilder sb = new();
        private readonly HashSet<string> dependencies = new();
        private readonly List<string> phaseOrder = new();
        private void AddDependency(string path)
        {
            if (dependencies.Add(path) && path != ctx.assetPath) ctx.DependsOnSourceAsset(path);
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var shaderModules = ProjectSettings.GetShaderModules(ctx.assetPath);

            this.ctx = ctx;
            var directory = Utils.GetDirectory(ctx.assetPath);
            modules.Add(SCModule.FromShaderFile(ctx.assetPath));
            modules.AddRange(AssetUtils.GetFiles("*.scmodule").Select(path => SCModule.FromFile(path)).Where(m => shaderModules.Contains(m.uniqueID)).OrderBy(m => m));
            phases.AddRange(modules.SelectMany(m => m.phases));

            ReadAndReplace(ctx.assetPath, directory, "");
            ReadAndReplaceProperty();

            // Minify HLSL
            using var sr = new StringReader(sb.ToString());
            sb.Clear();
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                sb.AppendLine(line);
                if (REG_HLSLINCLUDE.IsMatch(line) || REG_HLSLPROGRAM.IsMatch(line))
                {
                    var sb2 = new StringBuilder();
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (REG_ENDHLSL.IsMatch(line))
                        {
                            sb.AppendLine(HLSLMinifier.Minify(sb2.ToString()));
                            sb.AppendLine(line);
                            break;
                        }
                        sb2.AppendLine(line);
                    }
                }
            }

            var source = sb.ToString();
            var shader = ShaderUtil.CreateShaderAsset(ctx, source, false);
            var text = new TextAsset(source)
            {
                name = "Shader Source",
                hideFlags = HideFlags.HideInHierarchy
            };
            ctx.AddObjectToAsset("main obj", shader);
            ctx.AddObjectToAsset("Shader Source", text);
            ctx.SetMainObject(shader);

            AddDependency($"{Utils.RemoveExtension(ctx.assetPath)}_properties.hlsl");
            foreach (var module in modules)
            {
                if (string.IsNullOrEmpty(module.path)) continue;
                var moduleDirectory = Utils.GetDirectory(module.path);
                AddDependency(module.path);
                AddDependency(moduleDirectory + "properties.hlsl");
                AddDependency(moduleDirectory + "includes.hlsl");
                foreach (var phase in module.phases)
                    AddDependency(phase.path);
            }

            AssetDatabase.FindAssets("*.scmodule"); // Avoid Error
        }

        private void ReadAndReplaceProperty()
        {
            // phaseを持たないもの
            // シェーダー上での使用順
            // phaseが不明のもの
            // の順でソート
            var groups = modules.GroupBy(m => {
                var phase = m.phases.OrderBy(m => m).FirstOrDefault();
                return phase == null ? "" : phase.phase;
            });
            var orderdModules = new List<SCModule>();
            if (groups.FirstOrDefault(g => g.Key == "") is IGrouping<string, SCModule> g) orderdModules.AddRange(g);
            foreach (var p in phaseOrder)
                if (groups.FirstOrDefault(g => g.Key == p) is IGrouping<string, SCModule> g2) orderdModules.AddRange(g2);
            orderdModules.AddRange(groups.Where(g => !phaseOrder.Contains(g.Key) && g.Key != "").SelectMany(g => g));

            var sr = new StringReader(sb.ToString());
            sb.Clear();
            string line;
            while((line = sr.ReadLine()) != null)
            {
                var indent2 = REG_INDENT.Match(line).Value;
                if (ReplaceShaderKeywords(line, indent2, sb, orderdModules)) continue;
                if (ReplaceProperties(line, indent2, sb, orderdModules)) continue;
                if (ReplacePropertiesBIRP(line, indent2, sb, orderdModules)) continue;
                if (ReplacePropertiesURP(line, indent2, sb, orderdModules)) continue;
                sb.AppendLine(line);
            }
        }

        private void ReadAndReplace(string path, string directory, string indent)
        {
            using var sr = new StreamReader(path);
            string line;
            while((line = sr.ReadLine()) != null)
            {
                var indent2 = indent + REG_INDENT.Match(line).Value;
                if (RelacePhases(line, indent2, directory)) continue;
                if (RelaceInclude(line, indent2, directory)) continue;
                if (ReplaceModuleIncludes(line)) continue;
                sb.Append(indent).AppendLine(line);
            }
        }

        private void ReadAndReplaceText(string text, string directory, string indent)
        {
            using var sr = new StringReader(text);
            string line;
            while((line = sr.ReadLine()) != null)
            {
                var indent2 = indent + REG_INDENT.Match(line).Value;
                if (RelacePhases(line, indent2, directory)) continue;
                if (RelaceInclude(line, indent2, directory)) continue;
                if (ReplaceModuleIncludes(line)) continue;
                sb.Append(indent).AppendLine(line);
            }
        }

        private bool RelacePhases(string line, string indent, string directory)
        {
            var match = REG_PHASE.Match(line);
            if (!match.Success) return false;
            var phase = match.Groups[1].Value;
            if (!phaseOrder.Contains(phase)) phaseOrder.Add(phase);

            var sb2 = new StringBuilder();
            foreach (var p in phases.Where(p => p.phase == phase && File.Exists(p.path)))
                p.LoadHLSL(sb2, indent, "_ST");

            ReadAndReplaceText(sb2.ToString(), directory, "");

            return true;
        }

        private bool ReplaceModuleIncludes(string line)
        {
            var match = REG_INCLUDES.Match(line);
            if (!match.Success) return false;
            foreach (var m in modules)
                if (!string.IsNullOrEmpty(m.includes)) sb.AppendLine(m.includes);
            return true;
        }

        private bool RelaceInclude(string line, string indent, string directory)
        {
            var match = REG_INCLUDE.Match(line);
            if (!match.Success) return false;
            if (File.Exists($"{directory}{match.Groups[1].Value}"))
            {
                var include = $"{directory}{match.Groups[1].Value}";
                AddDependency(include);
                ReadAndReplace(include, Utils.GetDirectory(include), indent);
                return true;
            }
            if (File.Exists($"{Utils.GetDirectory(ctx.assetPath)}{match.Groups[1].Value}"))
            {
                var include = $"{Utils.GetDirectory(ctx.assetPath)}{match.Groups[1].Value}";
                AddDependency(include);
                ReadAndReplace(include, Utils.GetDirectory(include), indent);
                return true;
            }
            if (match.Groups[1].Value != "Packages/jp.lilxyzw.shadercore/ShaderLibrary/warnings.hlsl" && !match.Groups[1].Value.StartsWith("Packages/com.unity.") && File.Exists(match.Groups[1].Value))
            {
                var include = match.Groups[1].Value;
                AddDependency(include);
                ReadAndReplace(include, Utils.GetDirectory(include), indent);
                return true;
            }
            return false;
        }

        private static bool ReplaceShaderKeywords(string line, string indent, StringBuilder sb, IEnumerable<SCModule> modules)
        {
            if (!line.Contains("__SC_SHADERKEYWORDS__")) return false;
            foreach (var module in modules)
            {
                if(module.properties == null) continue;
                foreach (var prop in module.properties)
                {
                    if (prop.attributes == null) continue;
                    foreach (var attr in prop.attributes)
                    {
                        var match = REG_SCConstValue.Match(attr);
                        if (match.Success)
                        {
                            if (match.Groups[1].Value.Contains(','))
                            {
                                ToKeywords(indent, sb, prop, match.Groups[1].Value.Split(','));
                            }
                            else
                            {
                                ToKeywords(indent, sb, prop, match.Groups[1].Value);
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static void ToKeywords(string indent, StringBuilder sb, SCProperty prop, params string[] args)
        {
            if (!int.TryParse(args[0], out var max)) throw new Exception($"Invalid max value. {prop.name} => {args[0]}");
            if (args.Length == 1)
            {
                sb.Append(indent).Append(ToKeywords(prop, max, ""));
            }
            else
            {
                for (int i = 1; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "vertex": sb.Append(indent).AppendLine(ToKeywords(prop, max, "_vertex")); break;
                        case "pixel": sb.Append(indent).AppendLine(ToKeywords(prop, max, "_fragment")); break;
                        case "hull": sb.Append(indent).AppendLine(ToKeywords(prop, max, "_hull")); break;
                        case "domain": sb.Append(indent).AppendLine(ToKeywords(prop, max, "_domain")); break;
                        case "geometry": sb.Append(indent).AppendLine(ToKeywords(prop, max, "_geometry")); break;
                        default: throw new Exception($"Unknown shader stage. {prop.name} => {args[i]}");
                    }
                }
            }

            for (int i = 0; i <= max; i++)
            {
                if (i == 0) sb.Append(indent).AppendLine($"#if defined({prop.name.ToUpper(CultureInfo.InvariantCulture)}_{i})");
                else sb.Append(indent).AppendLine($"#elif defined({prop.name.ToUpper(CultureInfo.InvariantCulture)}_{i})");
                sb.Append(indent).AppendLine($"#define {prop.name} {i}");
            }
            sb.Append(indent).AppendLine($"#else");
            sb.Append(indent).AppendLine($"#define {prop.name} {0}");
            sb.Append(indent).AppendLine($"#endif");
        }

        private static string ToKeywords(SCProperty prop, int max, string postfix)
        {
            return $"#pragma shader_feature_local{postfix} " + string.Join(" ", Enumerable.Range(0, max+1).Select(i => $"{prop.name.ToUpper(CultureInfo.InvariantCulture)}_{i}"));
        }

        private static bool SkipProperty(SCProperty prop)
        {
            if (prop.attributes == null) return false;
            foreach (var attr in prop.attributes)
            {
                var match = REG_SCConstValue.Match(attr);
                if (match.Success) return true;
            }
            return false;
        }
    }
}
