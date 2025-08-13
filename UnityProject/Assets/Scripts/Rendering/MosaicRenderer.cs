using UnityEngine;
using UnityEngine.Rendering;

namespace SchoolLoveSimulator.Rendering
{
    /// <summary>
    /// FANZA/DMM規約準拠のモザイク処理システム
    /// 性器部分に自動でモザイクを適用
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MosaicRenderer : MonoBehaviour
    {
        [Header("Mosaic Settings")]
        [SerializeField] private bool enableMosaic = true;
        [SerializeField] private int mosaicSize = 16; // モザイクの粗さ（FANZA基準：最低16px）
        [SerializeField] private float mosaicStrength = 1.0f;
        
        [Header("Target Areas")]
        [SerializeField] private Transform[] mosaicTargets; // モザイク対象の部位
        [SerializeField] private Vector2 mosaicAreaSize = new Vector2(0.3f, 0.3f);
        
        private Camera targetCamera;
        private RenderTexture mosaicTexture;
        private Material mosaicMaterial;
        
        void Start()
        {
            InitializeMosaic();
            ValidateFANZACompliance();
        }
        
        void InitializeMosaic()
        {
            targetCamera = GetComponent<Camera>();
            
            // モザイク用シェーダー作成
            Shader mosaicShader = Shader.Find("Hidden/MosaicShader");
            if (mosaicShader == null)
            {
                CreateMosaicShader();
            }
            
            mosaicMaterial = new Material(mosaicShader);
            
            // レンダーテクスチャ設定
            mosaicTexture = new RenderTexture(
                Screen.width / mosaicSize,
                Screen.height / mosaicSize,
                0
            );
            mosaicTexture.filterMode = FilterMode.Point;
        }
        
        void CreateMosaicShader()
        {
            // カスタムモザイクシェーダー
            string shaderCode = @"
Shader ""Hidden/MosaicShader""
{
    Properties
    {
        _MainTex (""Texture"", 2D) = ""white"" {}
        _MosaicSize (""Mosaic Size"", Float) = 16
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include ""UnityCG.cginc""
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MosaicSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 mosaicUV = floor(i.uv * _MosaicSize) / _MosaicSize;
                fixed4 col = tex2D(_MainTex, mosaicUV);
                return col;
            }
            ENDCG
        }
    }
}";
            // シェーダーをリソースとして保存する必要があります
        }
        
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!enableMosaic || mosaicMaterial == null)
            {
                Graphics.Blit(source, destination);
                return;
            }
            
            // モザイク領域を計算
            RenderTexture temp = RenderTexture.GetTemporary(source.width, source.height);
            
            // 各ターゲットにモザイク適用
            Graphics.Blit(source, temp);
            
            foreach (Transform target in mosaicTargets)
            {
                if (target != null && IsTargetVisible(target))
                {
                    ApplyMosaicToArea(temp, target);
                }
            }
            
            Graphics.Blit(temp, destination);
            RenderTexture.ReleaseTemporary(temp);
        }
        
        bool IsTargetVisible(Transform target)
        {
            // ターゲットが画面内にあるかチェック
            Vector3 viewPos = targetCamera.WorldToViewportPoint(target.position);
            return viewPos.z > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1;
        }
        
        void ApplyMosaicToArea(RenderTexture texture, Transform target)
        {
            // ワールド座標をスクリーン座標に変換
            Vector3 screenPos = targetCamera.WorldToScreenPoint(target.position);
            
            // モザイク領域を定義
            Rect mosaicRect = new Rect(
                screenPos.x - (mosaicAreaSize.x * Screen.width / 2),
                screenPos.y - (mosaicAreaSize.y * Screen.height / 2),
                mosaicAreaSize.x * Screen.width,
                mosaicAreaSize.y * Screen.height
            );
            
            // シェーダーパラメータ設定
            mosaicMaterial.SetFloat("_MosaicSize", mosaicSize);
            mosaicMaterial.SetVector("_MosaicArea", new Vector4(
                mosaicRect.x / Screen.width,
                mosaicRect.y / Screen.height,
                mosaicRect.width / Screen.width,
                mosaicRect.height / Screen.height
            ));
            
            // モザイク適用
            RenderTexture temp = RenderTexture.GetTemporary(texture.width, texture.height);
            Graphics.Blit(texture, temp, mosaicMaterial);
            Graphics.Blit(temp, texture);
            RenderTexture.ReleaseTemporary(temp);
        }
        
        void ValidateFANZACompliance()
        {
            // FANZA/DMM規約チェック
            if (mosaicSize < 16)
            {
                Debug.LogError("モザイクサイズが FANZA/DMM 規約の最小値（16px）を下回っています！");
                mosaicSize = 16;
            }
            
            if (!enableMosaic)
            {
                Debug.LogWarning("モザイク処理が無効です。FANZA/DMM での販売には必須です。");
            }
        }
        
        public void SetMosaicTargets(Transform[] targets)
        {
            mosaicTargets = targets;
        }
        
        public void AddMosaicTarget(Transform target)
        {
            System.Array.Resize(ref mosaicTargets, mosaicTargets.Length + 1);
            mosaicTargets[mosaicTargets.Length - 1] = target;
        }
        
        public void RemoveMosaicTarget(Transform target)
        {
            var list = new System.Collections.Generic.List<Transform>(mosaicTargets);
            list.Remove(target);
            mosaicTargets = list.ToArray();
        }
        
        // エディタ用デバッグ表示
        void OnDrawGizmos()
        {
            if (mosaicTargets == null) return;
            
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            foreach (Transform target in mosaicTargets)
            {
                if (target != null)
                {
                    Gizmos.DrawCube(target.position, new Vector3(
                        mosaicAreaSize.x,
                        mosaicAreaSize.y,
                        0.1f
                    ));
                }
            }
        }
    }
}