﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace properties2csharp
{
    public static class ClassGenerator
    {
        public static Configuration DefaultConfiguration { get; set; } = new Configuration();
        public static string Generate(string propertiesInput, string name, Configuration Configuration)
        {
            Dictionary<string, bool> uses = new Dictionary<string, bool>();
            void AddUse(string u)
            {
                uses[u] = true;
            }
            AddUse("System");
            AddUse("Dalk.PropertiesSerializer");
            string GenForTp(string properties, string nm)
            {
                string s = "";
                var prop = properties.Replace("\r\n", "\n");
                var lines = prop.Split('\n');
                Dictionary<string, string> kvps = new Dictionary<string, string>();
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("#"))
                    {

                    }
                    else if (string.IsNullOrWhiteSpace(line))
                    {

                    }
                    else if (!line.Contains("="))
                    {

                    }
                    else
                    {
                        var eqp = line.IndexOf('=');
                        var key = line.Remove(eqp);
                        var value = line.Remove(0, eqp + 1);
                        kvps[key] = value;
                    }
                }
                List<GenProperty> props = new List<GenProperty>();
                Dictionary<string, string> customTypes = new Dictionary<string, string>();
                string GetName(string b)
                {
                    var r = new StringBuilder(b.Replace("-","_"));
                    r[0] = r[0].ToString().ToUpper().ToCharArray()[0];
                    while (r.ToString().Contains("_"))
                    {
                        var i = r.ToString().IndexOf('_');
                        r[i + 1] = r[i + 1].ToString().ToUpper().ToCharArray()[0];
                        r.Remove(i, 1);
                    }
                    return r.ToString();
                }
                Regex ints = new Regex(@"^[0-9]+$");
                bool IntOrPn(string st)
                {
                    var pcount = st.Length - st.Replace(".", "").Length;
                    if(pcount > 1)
                        return false;
                    return ints.IsMatch(st.Replace(".",""));
                }
                string GetType(string val)
                {
                    string r = Configuration.ObjectType;
                    if (ints.Match(val).Success)
                        r = Configuration.NumberType;
                    else if (IntOrPn(val))
                        r = Configuration.PointNumberType;
                    else if (IntOrPn(val.ToLower().Replace("f", "")))
                        r = Configuration.FloatType;
                    else if (val.ToLower() == "true")
                        r = Configuration.BoolType;
                    else if (val.ToLower() == "false")
                        r = Configuration.BoolType;
                    else if (DateTime.TryParse(val, out var _))
                        r = Configuration.DateType;
                    return r;
                }
                void AddLineAsProp(KeyValuePair<string, string> ln)
                {
                    GenProperty propx = new GenProperty()
                    {
                        Name = ln.Key,
                        PropertyName = GetName(ln.Key),
                        Type = GetType(ln.Value)
                    };
                    props.Add(propx);
                }
                string AsStr(Dictionary<string, string> d)
                {
                    string str = "";
                    foreach (var kvp in d)
                    {
                        str += kvp.Key + "=" + kvp.Value + "\n";
                    }
                    str = str.Remove(str.Length - 1);
                    return str;
                }
                void AddLineAsPropWithTypeName(KeyValuePair<string, string> ln, string tn)
                {
                    GenProperty propx = new GenProperty
                    {
                        Name = ln.Key,
                        PropertyName = GetName(ln.Key),
                        Type = tn
                    };
                    props.Add(propx);
                }
                foreach (var l in kvps)
                {
                    var k = l.Key;
                    if (k.Contains("."))
                    {
                        int i = k.IndexOf('.');
                        var tns = k.Remove(i);
                        var tn = GetName(tns);
                        CollectStarting(kvps, tns + ".", out var use, out var _);
                        if (!customTypes.ContainsKey(tn))
                        {
                            customTypes[tn] = GenForTp(AsStr(use), tn);
                            AddLineAsPropWithTypeName(new KeyValuePair<string, string>(tns, ""), tn);
                        }
                    }
                    else
                    {
                        AddLineAsProp(l);
                    }
                }
                s += $"    public class {nm}\n";
                s += "    {";
                foreach (var p in props)
                {
                    try
                    {
                        s += "\n";
                        if (p.Name != p.PropertyName)
                            s += $"        [PropertyName(\"{p.Name}\")]\n";
                        s += $"        public {p.Type} {p.PropertyName} ";
                        s += "{ get; set; }\n";
                    }
                    catch (Exception)
                    {

                    }
                }
                s += "    }";
                s += "\n";

                foreach (var c in customTypes)
                {
                    try
                    {
                        s += "\n" + c.Value + "\n";
                    }
                    catch (Exception)
                    {

                    }
                }
                return s;
            }
            var result = "";
            foreach (var u in uses)
            {
                result += $"using {u.Key};\n";
            }
            result += $"\nnamespace {Configuration.Namespace}\n";
            result += "{";
            result += "\n" + GenForTp(propertiesInput, name);
            result += "}";
            result = result.TrimEnd();
            return result;
        }


        private static void CollectStarting(Dictionary<string, string> dict, string startString, out Dictionary<string, string> starting, out Dictionary<string, string> other)
        {
            Dictionary<string, string> st = new Dictionary<string, string>();
            Dictionary<string, string> ot = new Dictionary<string, string>();
            foreach (var c in dict)
            {
                if (c.Key.StartsWith(startString))
                {
                    st.Add(c.Key.Remove(0, startString.Length), c.Value);
                }
                else
                {
                    ot.Add(c.Key, c.Value);
                }
            }
            starting = st;
            other = ot;
        }
    }
}