using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Template {
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    struct Light {
        [FieldOffset(0)]
        private Vector4 intensity;
        [FieldOffset(16)]
        private Vector3 location;
        public Light(Vector3 location, Vector4 intensity)
        {
            this.location = location;
            this.intensity = intensity;
        }

        public Vector3 Location { get { return location; } }
        public Vector4 Intensity { get { return intensity; } }
    }
}
