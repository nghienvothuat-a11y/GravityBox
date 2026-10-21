using UnityEngine;
namespace GravityBox.Venom
{
    public sealed partial class VenomLifeAnimation
    {
        COgheReadingBook readingBook;
        public Vector3? ReadingFocus=>readingBook!=null ? readingBook.transform.TransformPoint(new Vector3(Mathf.Sin(level.Organism.SimulationTime*1.7f)*.014f,.004f,0)) : (Vector3?)null;
        int ReadingAct(COghePersonality p,Vector3[] points,int count,Vector3 ground,Vector3 floor,float t)
        {
            float e=Smooth(0,.65f,t)*(1-Smooth(p.Length-.65f,p.Length,t))*p.Fade;
            if(readingBook==null)
            {
                var go=new GameObject("Clever · reading book (presentation only)");go.transform.SetParent(transform,false);
                readingBook=go.AddComponent<COgheReadingBook>();readingBook.Initialize(p,level.MatterProfile.Skin);
            }
            // Two soft arms retrieve the book from the liquid, open it and cradle the edges. No source count increase.
            PoseBody(points,count,ground,1-.07f*e,Quaternion.AngleAxis(5*e,actRight),Vector3.zero,1,e);
            Vector3 book=ground+actUp*(.016f+.034f*e)+actToCamera*(.015f+.035f*e);
            readingBook.Draw(book,Quaternion.LookRotation(actToCamera,actUp)*Quaternion.Euler(22,0,0),e,t);
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 root=ground+actUp*.018f+actRight*(side*.018f);
                Vector3 tip=readingBook.transform.TransformPoint(new Vector3(side*.030f,.002f,.012f));
                tip=Vector3.Lerp(root,tip,e);
                Limb(root,root+actRight*(side*.021f*e),tip-actUp*.012f,tip,.006f*e,.0032f*e,floor,actUp);
            }
            HeadAmount=Mathf.Max(HeadAmount,e*.35f);return count;
        }
    }
    public sealed class COgheReadingBook : MonoBehaviour
    {
        COghePersonality personality;COgheHomeMaterials materials;Material lit;
        Transform left,right,page;GameObject visual;
        public bool Visible=>visual!=null&&visual.activeSelf;
        public void Initialize(COghePersonality p,Material source)
        {
            personality=p;lit=new Material(source);materials=new COgheHomeMaterials(lit,lit);
            visual=new GameObject("Open storybook");visual.transform.SetParent(transform,false);
            left=Half(-1);right=Half(1);
            page=new GameObject("Turning page").transform;page.SetParent(visual.transform,false);
            var k=new COgheLowPoly();k.Paint("#FFF4D9").Box(new Vector3(.015f,.0042f,0),new Vector3(.028f,.0005f,.037f),.0002f);
            k.Bake("Paper",page,materials);
        }
        Transform Half(int side)
        {
            var pivot=new GameObject(side<0?"Left cover":"Right cover").transform;pivot.SetParent(visual.transform,false);
            var k=new COgheLowPoly();
            k.Paint("#397F82").Box(new Vector3(side*.016f,0,0),new Vector3(.033f,.003f,.043f),.0012f);
            k.Paint("#FFF1CD").Box(new Vector3(side*.016f,.0025f,0),new Vector3(.030f,.003f,.039f),.0007f);
            // Quiet printed lines and a gold corner; no textures, lights or physics components.
            for(int i=0;i<4;i++)k.Paint("#96B7AF").Box(new Vector3(side*.016f,.0041f,-.010f+i*.006f),new Vector3(i==3?.014f:.021f,.0002f,.001f),0);
            k.Paint("#E8BC64").Box(new Vector3(side*.028f,.004f,.016f),new Vector3(.003f,.0003f,.004f),.0004f);
            k.Bake("Bound pages",pivot,materials);return pivot;
        }
        public void Draw(Vector3 position,Quaternion rotation,float amount,float time)
        {
            visual.SetActive(amount>.005f);transform.SetPositionAndRotation(position,rotation);transform.localScale=Vector3.one*Mathf.Lerp(.12f,1,amount);
            float angle=Mathf.Lerp(82,9,Mathf.SmoothStep(0,1,amount));
            left.localRotation=Quaternion.Euler(0,0,-angle);right.localRotation=Quaternion.Euler(0,0,angle);
            float turn=Mathf.SmoothStep(0,1,Mathf.Clamp01((time-2.1f)/1.2f));
            page.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(9,171,turn));
        }
        void LateUpdate(){if(personality==null||personality.Act!=COgheAct.Read)visual?.SetActive(false);}
        void OnDestroy(){materials?.Dispose();if(lit!=null)Destroy(lit);}
    }
}
