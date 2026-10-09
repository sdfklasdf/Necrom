using UnityEngine;
namespace Necrom.ContentDraft {
 // Opt-in 3D preparation scene only; no gameplay/roster behavior.
 [DisallowMultipleComponent,RequireComponent(typeof(Camera))]
 public sealed class MobileQuarterView : MonoBehaviour {
 public Vector3 boardCenter=new Vector3(0,.9f,0);
 public Vector3 boardExtents=new Vector3(3.5f,.9f,8);
 public Vector3 viewAngles=new Vector3(45,45,0);
 public float boardYaw=45, distance=24, framingMargin=1.06f, shadowDistance=40;
 public Light mainLight;
 bool applied;float lastAspect;
 ShadowQuality oldShadows;ShadowResolution oldResolution;
 float oldDistance;int oldCascades,oldLights,oldAA;
 bool oldParticles,oldReflections;
 void OnEnable(){if(Application.isPlaying){ApplyMobileQuality();Configure(GetComponent<Camera>().aspect);}}
 void LateUpdate(){var c=GetComponent<Camera>();if(!Mathf.Approximately(lastAspect,c.aspect))Configure(c.aspect);}
 void OnDisable(){RestoreQuality();}
 public void Configure(float aspect) {
 var c=GetComponent<Camera>();lastAspect=Mathf.Max(.1f,aspect);
 var rotation=Quaternion.Euler(viewAngles);transform.rotation=rotation;
 transform.position=boardCenter-rotation*Vector3.forward*Mathf.Max(10,distance);
 c.orthographic=true;c.aspect=lastAspect;c.fieldOfView=45;
 c.nearClipPlane=.3f;c.farClipPlane=60;c.allowHDR=false;c.allowMSAA=true;
 c.renderingPath=RenderingPath.Forward;c.depthTextureMode=DepthTextureMode.None;
 float xMax=0,yMax=0;
 foreach(var x in new[]{-1,1})foreach(var y in new[]{-1,1})foreach(var z in new[]{-1,1}) {
 var offset=Quaternion.Euler(0,boardYaw,0)*Vector3.Scale(boardExtents,new Vector3(x,y,z));
 var local=Quaternion.Inverse(rotation)*offset;xMax=Mathf.Max(xMax,Mathf.Abs(local.x));yMax=Mathf.Max(yMax,Mathf.Abs(local.y));
 }
 c.orthographicSize=Mathf.Max(yMax,xMax/lastAspect)*Mathf.Max(1.01f,framingMargin);
 if(mainLight!=null) {
 mainLight.type=LightType.Directional;mainLight.shadows=LightShadows.Hard;
 mainLight.shadowCustomResolution=1024;mainLight.shadowStrength=.65f;
 mainLight.shadowBias=.05f;mainLight.shadowNormalBias=.3f;mainLight.renderMode=LightRenderMode.ForcePixel;
 }
 }
 public void ApplyMobileQuality() {
 if(applied)return;
 oldShadows=QualitySettings.shadows;oldResolution=QualitySettings.shadowResolution;
 oldDistance=QualitySettings.shadowDistance;oldCascades=QualitySettings.shadowCascades;
 oldLights=QualitySettings.pixelLightCount;oldAA=QualitySettings.antiAliasing;
 oldParticles=QualitySettings.softParticles;oldReflections=QualitySettings.realtimeReflectionProbes;
 applied=true;
 QualitySettings.shadows=ShadowQuality.HardOnly;QualitySettings.shadowResolution=ShadowResolution.Medium;
 QualitySettings.shadowDistance=shadowDistance;QualitySettings.shadowCascades=1;
 QualitySettings.pixelLightCount=1;QualitySettings.antiAliasing=2;
 QualitySettings.softParticles=false;QualitySettings.realtimeReflectionProbes=false;
 }
 public void RestoreQuality() {
 if(!applied)return;applied=false;
 QualitySettings.shadows=oldShadows;QualitySettings.shadowResolution=oldResolution;
 QualitySettings.shadowDistance=oldDistance;QualitySettings.shadowCascades=oldCascades;
 QualitySettings.pixelLightCount=oldLights;QualitySettings.antiAliasing=oldAA;
 QualitySettings.softParticles=oldParticles;QualitySettings.realtimeReflectionProbes=oldReflections;
 }
 }
}
