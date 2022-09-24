using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SC
{
    class ConfigFileReader2 : IDisposable
    {
        private XmlReader Reader_;
        public configFile ConfigContent { get; set; } = new configFile();

        public ConfigFileReader2(Stream stream)
        {
            Reader_ = XmlReader.Create(stream, new XmlReaderSettings { CloseInput = true });

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "SC_ConfigfFile":
                                ReadConfig();
                                break;
                            default:
                                SkipElement();
                                break; 
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != "SC_ConfigfFile") throw new FormatException(Reader_.Name);
                        return;
                }
            }
        }

        public void Dispose()
        {
            Reader_.Close();
        }

        private void ReadConfig()
        {
            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Api-Key":
                                ReadApiKey();
                                break;
                            case "Directory":
                                ReadDir();
                                break;
                            case "StartPoint":
                                ReadStart();
                                break;
                            case "Category":
                                ConfigContent.Categories = ReadCat();
                                break;
                            case "Weather":
                                ConfigContent.Weather = ReadCond();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != "SC_ConfigfFile") throw new FormatException(Reader_.Name + "YYY");
                        return;
                }
            }
        }

        private void ReadApiKey()
        {
            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Landscape":
                        ConfigContent.ApiKey_Landscape = Reader_.Value;
                        break;
                    case "Satelite":
                        ConfigContent.ApiKey_Satelite = Reader_.Value;
                        break;
                    case "Hybride":
                        ConfigContent.ApiKey_Hybride = Reader_.Value;
                        break;
                    case "Weather":
                        ConfigContent.ApiKey_Weather = Reader_.Value;
                        break;
                }
            }
        }
        private void ReadDir()
        {
            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "WorkDir":
                        ConfigContent.WorkDir = Reader_.Value;
                        break;
                    case "RoutDir":
                        ConfigContent.RoutesDir = Reader_.Value;
                        break;
                    case "SRTMDir":
                        ConfigContent.SRTMDir = Reader_.Value;
                        break;
                }
            }
        }
        private void ReadStart()
        {
            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Lon":
                        ConfigContent.StartPoint_Lon = Reader_.Value;
                        break;
                    case "Lat":
                        ConfigContent.StartPoint_Lat = Reader_.Value;
                        break;
                    case "Zoom":
                        ConfigContent.StartPoint_Zoom = Reader_.Value;
                        break;
                }
            }
        }

        private List<String> ReadCat()
        {
            List<String> cat_list = new List<String>();
            if (Reader_.IsEmptyElement) return cat_list;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "cat":
                                cat_list.Add(Reader_.GetAttribute("Value"));
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        return cat_list;
                }
            }

            return cat_list;
        }

        private List<String> ReadCond()
        {
            List<String> cond_list = new List<String>();
            if (Reader_.IsEmptyElement) return cond_list;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "cond":
                                cond_list.Add(Reader_.GetAttribute("Value"));
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        return cond_list;
                }
            }

            return cond_list;
        }

        private void SkipElement()
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;
            int depth = Reader_.Depth;

            while (Reader_.Read())
            {
                if (Reader_.NodeType == XmlNodeType.EndElement)
                {
                    if (Reader_.Depth == depth && Reader_.Name == elementName) return;
                }
            }

            throw new FormatException(elementName);
        }
    }
}
