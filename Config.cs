using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ComBot_Revamped
{
    [Serializable]
    internal class Config
    {
        // Be aware if this is false you must use your raw bot token in the JSON
        public bool? requirePassword { get; set; }
        public string? token { get; set; } // encrypted using the password hash.
        public string? password { get; set; } // Hashed. Of course.
        public string? tag { get; set; }
        public string? hash { get; set; } // Make sure nothing's corrupted
        public string? nonce { get; set; } // heck.
        public string? salt { get; set; }



    }
}
