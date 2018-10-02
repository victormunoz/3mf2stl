using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            bool ascii_output_flag = true;
            string infilename = null;
            string outfilename = null;

            bool error = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-a":
                        ascii_output_flag = true;
                        break;
                    case "-i":
                        if (i + 1 < args.Length)
                            infilename = args[++i];
                        else error = true;
                        break;
                    case "-o":
                        if (i + 1 < args.Length)
                            outfilename = args[++i];
                        else error = true;
                        break;
                    default:
                        error = true;
                        break;
                }
            }
            if (infilename == null || outfilename == null) error = true;
            if (error)
            {
                usage();
                return;
            }

            string content = get_model(infilename);
            if (content == null) return;

            List<Triangle> triangles = new List<Triangle>();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            var model = doc.DocumentElement;
            if (model != null && model.Name == "model")
            {
                var resources = getChild(model, "resources");
                if (resources != null)
                {
                    var objs = getChilds(resources, "object");
                    foreach (var obj in objs)
                    {
                        var mesh = getChild(obj, "mesh");
                        if (mesh != null)
                        {
                            List<Vertex> vertices = new List<Vertex>();
                            var verts = getChild(mesh, "vertices");
                            if (verts != null)
                            {
                                var vertexs = getChilds(verts, "vertex");
                                foreach (var vertex_child in vertexs)
                                {
                                    Vertex vertex = new Vertex();

                                    vertex.x = double.Parse(vertex_child.Attributes["x"].Value, CultureInfo.InvariantCulture);
                                    vertex.y = double.Parse(vertex_child.Attributes["y"].Value, CultureInfo.InvariantCulture);
                                    vertex.z = double.Parse(vertex_child.Attributes["z"].Value, CultureInfo.InvariantCulture);

                                    vertices.Add(vertex);
                                }
                            }

                            var tris = getChild(mesh, "triangles");
                            if (tris != null)
                            {
                                var tris_child_list = getChilds(tris, "triangle");
                                foreach (var tris_child in tris_child_list)
                                {
                                    Triangle tri = new Triangle();

                                    tri.Vertexs[0] = vertices[int.Parse(tris_child.Attributes["v1"].Value)];
                                    tri.Vertexs[1] = vertices[int.Parse(tris_child.Attributes["v2"].Value)];
                                    tri.Vertexs[2] = vertices[int.Parse(tris_child.Attributes["v3"].Value)];

                                    triangles.Add(tri);
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Read " + triangles.Count() + " triangles...");

            if (ascii_output_flag)
                save_to_ASCII_stereo_lithography_file(triangles, outfilename);
            //else
            //    save_to_binary_stereo_lithography_file(triangles, outfilename);
        }

        static XmlNode getChild(XmlNode node, string name)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == name)
                {
                    return child;
                }
            }
            return null;
        }

        static List<XmlNode> getChilds(XmlNode node, string name)
        {
            var list = new List<XmlNode>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == name)
                {
                    list.Add(child);
                }
            }
            return list;
        }

        static void usage()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            //Console.WriteLine("Usage: 3mf2stl [-a] -i infile -o outfile");
            Console.WriteLine("Usage: 3mf2stl -i infile -o outfile");
            Console.WriteLine("Convert the .3mf file infile to the stl file outfile.");
            //Console.WriteLine("The optional -a flag will create an ASCII (human readable) STL file." + Environment.NewLine);
            Console.WriteLine("Copyright Victor Muñoz, Charles Shapiro and Shawn Halayka Oct 2018");
            Console.WriteLine("Version " + version);
        }

        static string get_model(string filename)
        {
            string extractPath = "tmp";
            if (!Directory.Exists(extractPath)) Directory.CreateDirectory(extractPath);
            else Directory.Delete(extractPath, true);
            ZipFile.ExtractToDirectory(filename, extractPath);
            string modelDir = extractPath + "/3D";
            string modelFile = null;
            string contents = null;
            if (Directory.Exists(modelDir))
            {
                var files = Directory.GetFiles(modelDir);
                foreach (var file in files)
                {
                    if (file.EndsWith(".model"))
                    {
                        modelFile = file;
                        break;
                    }
                }
                if (modelFile != null)
                {
                    contents = File.ReadAllText(modelFile);
                }

                Directory.Delete(extractPath, true);
            }

            return contents;
        }

        static bool save_to_ASCII_stereo_lithography_file(List<Triangle> triangles, string file_name)
        {
            if (triangles.Count == 0)
                return false;

            // Write to file.
            StreamWriter f = new StreamWriter(file_name, false);

            f.WriteLine("solid triangles");

            for (var i = 0; i < triangles.Count; i++)
            {
                var a = triangles[i].Vertexs[2].subtract(triangles[i].Vertexs[0]);
                var b = triangles[i].Vertexs[1].subtract(triangles[i].Vertexs[0]);
                var normal = a.cross(b);
                normal.normalize();

                f.WriteLine("  facet normal " + normal.X() + ", " + normal.Y() + ", " + normal.Z());
                f.WriteLine("    outer loop");

                f.WriteLine("      vertex " + triangles[i].Vertexs[0].X() + ' ' + triangles[i].Vertexs[0].Y() + ' ' + triangles[i].Vertexs[0].Z());
                f.WriteLine("      vertex " + triangles[i].Vertexs[1].X() + ' ' + triangles[i].Vertexs[1].Y() + ' ' + triangles[i].Vertexs[1].Z());
                f.WriteLine("      vertex " + triangles[i].Vertexs[2].X() + ' ' + triangles[i].Vertexs[2].Y() + ' ' + triangles[i].Vertexs[2].Z());

                f.WriteLine("    endloop");
                f.WriteLine("  endfacet");
            }

            f.WriteLine("endsolid");

            f.Close();

            return true;
        }
    }
}
