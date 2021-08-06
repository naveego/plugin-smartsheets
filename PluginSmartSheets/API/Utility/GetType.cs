using System;
using Naveego.Sdk.Plugins;

namespace PluginSmartSheets.API.Utility
{
    public static class GetType
    {
        public static PropertyType GetPropertyType(string type)
        {
            switch (type)
            {
                case("DATE"):
                    return PropertyType.Date;
                case("DATETIME"):
                case("ABSTRACT_DATETIME"):
                    return PropertyType.Datetime;
                case("CHECKBOX"):
                case("CONTACT_LIST"):
                case("MULTI_CONTACT_LIST"):
                case("PICKLIST"):
                case("MULTI_PICKLIST"):
                case("PREDECESSOR"):
                case("DURATION"):
                case("TEXT_NUMBER"):
                    return PropertyType.String;
                    
                default:
                    return PropertyType.String;
            }
        }
    }
}