using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class OptimizeParticlesAndMaterials : EditorWindow
{
    [MenuItem("Tools/Optimize/Generate Particle+Material Report")]
    public static void GenerateReport()
    {
        var report = Scan(false);
        var outPath = "Assets/OptimizationReports/particle_material_report.txt";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, report);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("优化报告已生成", "报告已写入: " + outPath, "OK");
    }

    [MenuItem("Tools/Optimize/Apply Particle & Material Safe Optimizations")]
    public static void ApplyOptimizations()
    {
        if (!EditorUtility.DisplayDialog("确认应用优化", "将对项目中的预制体/材质进行安全性优化（会修改资产）。建议先用‘Generate Report’备份并检查。要继续吗？", "继续", "取消"))
            return;

        var report = Scan(true);
        var outPath = "Assets/OptimizationReports/particle_material_report_after_apply.txt";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, report);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("优化完成", "优化已应用，报告已写入: " + outPath, "OK");
    }

    static string Scan(bool applyChanges)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Particle & Material Scan Report");
        sb.AppendLine("Generated: " + System.DateTime.Now.ToString());
        sb.AppendLine();

        // Scan prefabs for ParticleSystem
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int totalPrefabs = prefabGuids.Length;
        int prefabsWithParticles = 0;
        int totalParticleSystems = 0;

        foreach (var g in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            var systems = go.GetComponentsInChildren<ParticleSystem>(true);
            if (systems != null && systems.Length > 0)
            {
                prefabsWithParticles++;
                sb.AppendLine("Prefab: " + path + " -> ParticleSystems: " + systems.Length);
                foreach (var ps in systems)
                {
                    totalParticleSystems++;
                    var main = ps.main;
                    sb.AppendLine("  - " + ps.name + " maxParticles=" + main.maxParticles + " simulationSpace=" + main.simulationSpace);

                    if (applyChanges)
                    {
                        bool changed = false;
                        // Cap maxParticles to a sane default if too large
                        int cap = 1500;
                        if (main.maxParticles > cap)
                        {
                            main.maxParticles = cap;
                            changed = true;
                        }
                        // Disable lights usage for performance
                        var lights = ps.lights;
                        if (lights.enabled)
                        {
                            lights.enabled = false;
                            changed = true;
                        }
                        // Use auto random seed for determinism
                        if (!ps.useAutoRandomSeed)
                        {
                            ps.useAutoRandomSeed = true;
                            changed = true;
                        }
                        if (changed)
                        {
                            EditorUtility.SetDirty(ps);
                            sb.AppendLine("    -> Optimized");
                        }
                    }
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("Prefabs scanned: " + totalPrefabs);
        sb.AppendLine("Prefabs with ParticleSystem: " + prefabsWithParticles);
        sb.AppendLine("Total ParticleSystems: " + totalParticleSystems);

        // Scan materials
        var matGuids = AssetDatabase.FindAssets("t:Material");
        int totalMaterials = matGuids.Length;
        int materialsChanged = 0;
        sb.AppendLine();
        sb.AppendLine("Materials scanned: " + totalMaterials);

        foreach (var mg in matGuids)
        {
            var mpath = AssetDatabase.GUIDToAssetPath(mg);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(mpath);
            if (mat == null) continue;
            var shaderName = mat.shader != null ? mat.shader.name : "<null>";
            sb.AppendLine("Material: " + mpath + " -> Shader: " + shaderName + " instancing=" + mat.enableInstancing);

            if (applyChanges)
            {
                bool changed = false;
                // Enable GPU instancing where supported
                if (!mat.enableInstancing)
                {
                    mat.enableInstancing = true;
                    changed = true;
                }
                if (changed)
                {
                    EditorUtility.SetDirty(mat);
                    materialsChanged++;
                    sb.AppendLine("    -> Enabled GPU Instancing");
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("Materials changed: " + materialsChanged);

        // List custom shaders
        var shaderGuids = AssetDatabase.FindAssets("t:Shader");
        sb.AppendLine();
        sb.AppendLine("Shaders found: " + shaderGuids.Length);
        foreach (var sg in shaderGuids)
        {
            var sp = AssetDatabase.GUIDToAssetPath(sg);
            sb.AppendLine(" - " + sp);
        }

        sb.AppendLine();
        sb.AppendLine("End of report");
        return sb.ToString();
    }
}
