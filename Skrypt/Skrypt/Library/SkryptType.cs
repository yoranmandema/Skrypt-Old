﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;

namespace Skrypt.Library {
    public class SkryptType : SkryptObject {
        public string TypeName { get { return GetType().ToString(); } }
        public bool CreateCopyOnAssignment = false;

        public void Init (SkryptEngine Engine) {
            SkryptObject BaseObject = Engine.Types[TypeName];

            SetPropertiesTo(BaseObject);
        }
    }
}