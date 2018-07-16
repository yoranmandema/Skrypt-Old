﻿using System;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant, Static]
        public class Void : SkryptObject
        {
            public Void()
            {
                Name = "void";
            }

            public override Boolean ToBoolean()
            {
                throw new Exception("Can't convert void to boolean!");
            }
        }
    }
}