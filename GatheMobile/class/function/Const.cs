using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Xamarin.Forms;

namespace GatheMobile
{
    public class Const
    {
        public static bool adminAccess = true, allowEditEvent = false;
        public static bool isTest = false;
        public static double defaultScreenHeight = 1280;
        public static double defaultScreenWidth = 720;
        public static string
            client_id = "",
            client_secret = "",
            BaseAddressWS = (isTest ? "http://localhost/api/" : "http://api.gathe.com/");
        //url
        //response
        public const string
            ResponseFailed = "failed";

        //status
        public const string
            StatusActive = "ACTIVE",
            StatusInactive = "INACTIVE";

        public const string
            path_image = "Image/",
            path_file = "File/";

        public const string
            events_id = "events",
            filter_id = "filter",
            people_id = "people",
            place_id = "place";

        //category
        public const string
            other = "others",
            sport = "sports",
            cultural = "cultural",
            party = "party",
            gaming = "gaming",
            conference = "conference",
            music = "music",
            charities = "charities";
    }
}
