using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Stable overview cameras for the two direct-control experiments.</summary>
    public static class VenomCameraFraming
    {
        // 02: read/select both pieces like a tabletop puzzle.
        // 03: look from the spawn end toward the outlet; raised gates move
        // away from the approaching body in screen space instead of covering it.
        // Keep a fixed heading so input directions never drift during a hold.
        public static Quaternion Orientation(VenomControlMode mode) =>
            mode == VenomControlMode.SelectFragment ? Quaternion.Euler(88,0,0) : Quaternion.Euler(58,192,0);

        public static void Frame(VenomLevelController level, int width, int height)
        {
            if (!level.DirectControl || width <= 0 || height <= 0) return;
            Camera camera = level.View;
            camera.orthographic = true;
            camera.transform.rotation = Orientation(level.ControlMode);
            float scale = width/540f;
            float bottom = 282*scale, top = height-205*scale;
            float available = Mathf.Max(80*scale,top-bottom);
            Vector3 right = camera.transform.right, up = camera.transform.up;
            float minX = float.PositiveInfinity, minY = minX, maxX = float.NegativeInfinity, maxY = maxX;
            void Include(Vector3 local)
            {
                Vector3 p = level.Rotation.transform.TransformVector(local);
                float x = Vector3.Dot(p,right), y = Vector3.Dot(p,up);
                minX = Mathf.Min(minX,x); maxX = Mathf.Max(maxX,x);
                minY = Mathf.Min(minY,y); maxY = Mathf.Max(maxY,y);
            }
            // Fit the chamber plus the actual high points of raised mechanisms,
            // rather than padding the entire roof up to the height of the blade.
            // Fixed bounds avoid zooming whenever the blade/gate moves.
            for (int x=-1;x<=1;x+=2) for (int y=0;y<=1;y++) for (int z=-1;z<=1;z+=2)
                Include(new Vector3(x*.262f,y==0?-.076f:.078f,z*.33f));
            for (int x=-1;x<=1;x+=2) for (int z=-1;z<=1;z+=2)
            {
                Include(level.BladeRest+new Vector3(x*.008f,.126f,z*.059f));
                Include(level.GateRest+new Vector3(x*.254f,.215f,z*.01f));
            }
            const float margin = .012f;
            float size = Mathf.Max(((maxY-minY)*.5f+margin)*height/available,
                ((maxX-minX)*.5f+margin)*height/(width*.92f));
            Vector3 centre = level.Rotation.transform.position+right*((minX+maxX)*.5f)+up*((minY+maxY)*.5f);
            camera.orthographicSize = size;
            camera.transform.position = centre-camera.transform.forward*1.3f-up*(((bottom+top)/height-1f)*size);
        }

        public static Vector3 ScreenToFloor(Camera camera, Vector2 direction)
        {
            Vector3 right = Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized;
            Vector3 forward = Vector3.Cross(right,Vector3.up).normalized;
            // Undo the foreshortening of the floor at an oblique angle. Preserve
            // joystick strength: diagonal gestures change heading, not top speed.
            float verticalScale = Mathf.Max(.1f,Vector3.Dot(camera.transform.up,forward));
            Vector3 projected = right*direction.x+forward*(direction.y/verticalScale);
            return projected.sqrMagnitude > 0 ? projected.normalized*Mathf.Clamp01(direction.magnitude) : Vector3.zero;
        }
    }
}
