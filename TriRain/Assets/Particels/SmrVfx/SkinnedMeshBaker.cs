using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Smrvfx
{
    public sealed class SkinnedMeshBaker : MonoBehaviour
    {
        #region Editable attributes


        [SerializeField] SkinnedMeshRenderer _source = null;
		[SerializeField] MeshFilter _altSource = null;
		Transform sourceTransform
		{
			get
			{
				if (_source != null)
					return _source.transform;
				else if (_altSource != null)
					return _altSource.transform;
				else
					return null;
			}
		}
		[Space]

        [SerializeField] RenderTexture _positionMap = null;
        [SerializeField] RenderTexture _velocityMap = null;
		[SerializeField] RenderTexture _colorMap = null;
		[SerializeField] RenderTexture _normalMap = null;
		[SerializeField] Texture2D _textureMap = null;
        [SerializeField] ComputeShader _compute = null;

        #endregion

        #region Temporary objects

        Mesh _mesh;
        Matrix4x4 _previousTransform = Matrix4x4.identity;

        int[] _dimensions = new int[2];

        List<Vector3> _positionList = new List<Vector3>();
		List<Vector3> _normalList = new List<Vector3>();
		List<Vector3> _uvList = new List<Vector3>();

		List<Vector3> _triList = new List<Vector3>();
		List<int> _triVertIndecies = new List<int>();
		List<List<int>> _includedTris = new List<List<int>>();

		ComputeBuffer _positionBuffer1;
        ComputeBuffer _positionBuffer2;
        ComputeBuffer _normalBuffer;
		ComputeBuffer _colorBuffer;


		RenderTexture _tempPositionMap;
        RenderTexture _tempVelocityMap;
        RenderTexture _tempNormalMap;
		RenderTexture _tempColorMap;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _mesh = new Mesh();

			if (_source != null)
				_source.BakeMesh(_mesh);
			else if (_altSource != null)
				_mesh = _altSource.mesh;
			else
				return;

			UpdateTriList();
			GetIncludedTrianglesPerVertex();

		}

		void OnDestroy()
        {
            Destroy(_mesh);
            _mesh = null;

            Utility.TryDispose(_positionBuffer1);
            Utility.TryDispose(_positionBuffer2);
            Utility.TryDispose(_normalBuffer);
			Utility.TryDispose(_colorBuffer);

			Utility.TryDestroy(_tempPositionMap);
            Utility.TryDestroy(_tempVelocityMap);
            Utility.TryDestroy(_tempNormalMap);
			Utility.TryDestroy(_tempColorMap);

			_positionBuffer1 = null;
            _positionBuffer2 = null;
            _normalBuffer = null;
			_colorBuffer = null;

			_tempPositionMap = null;
            _tempVelocityMap = null;
            _tempNormalMap = null;
			_tempColorMap = null;
		}

		void Update()
		{
			if (_source != null)
				_source.BakeMesh(_mesh);
			else if (_altSource != null)
				_mesh = _altSource.mesh;
			else
				return;
			
			_mesh.GetVertices(_positionList);
			_mesh.GetNormals(_normalList);
			_mesh.GetUVs(0,_uvList);



			if (!CheckConsistency()) return;

            TransferData();

            Utility.SwapBuffer(ref _positionBuffer1, ref _positionBuffer2);
            _previousTransform = sourceTransform.localToWorldMatrix;
        }


		void UpdateTriList()
		{ 
			_triList.Clear();
			_triVertIndecies = new List<int>();
			_mesh.GetTriangles(_triVertIndecies, 0);
			int tricount = _triVertIndecies.Count / 3;
			for(int i=0; i < tricount; i++)
			{
				int ti = i * 3;
				_triList.Add(new Vector3(_triVertIndecies[ti], _triVertIndecies[ti + 1], _triVertIndecies[ti + 2]));
			}
		}


		void GetIncludedTrianglesPerVertex()
		{
			_includedTris.Clear();
			string debug = "included tris:\n";
			int cnt = 0;
			int max = 0;
			for(int vInd =0; vInd < _mesh.vertexCount; vInd++)
			{
				var indeciesFound = _triVertIndecies.Select((b, i) => b == vInd ? i : -1).Where(i => i != -1).ToList();
				_includedTris.Add(indeciesFound);
				cnt += indeciesFound.Count;
				if (indeciesFound.Count > max)
					max = indeciesFound.Count;
				//debug += "\t i" + vInd + "->" + indeciesFound.Count;
			}

			debug += "\n-------\n counted:" + cnt + " =?= actual:" + _triVertIndecies.Count + " \n posCount: " + _positionList.Count + " =?= mshvCount: " + _mesh.vertexCount;
			debug += "\n mx:" + max + "\n";
			print(debug);
		}

        #endregion

			#region Private methods

        void TransferData()
        {
            var mapWidth = _positionMap.width;
            var mapHeight = _positionMap.height;

            var vcount = _positionList.Count;
            var vcount_x3 = vcount * 3;

            // Release the temporary objects when the size of them don't match
            // the input.

            if (_positionBuffer1 != null &&
                _positionBuffer1.count != vcount_x3)
            {
                _positionBuffer1.Dispose();
                _positionBuffer2.Dispose();
                _normalBuffer.Dispose();
				_colorBuffer.Dispose();

                _positionBuffer1 = null;
                _positionBuffer2 = null;
                _normalBuffer = null;
				_colorBuffer = null;
            }

            if (_tempPositionMap != null &&
                (_tempPositionMap.width != mapWidth ||
                 _tempPositionMap.height != mapHeight))
            {
                Destroy(_tempPositionMap);
                Destroy(_tempVelocityMap);
                Destroy(_tempNormalMap);
				Destroy(_tempColorMap);

                _tempPositionMap = null;
                _tempVelocityMap = null;
                _tempNormalMap = null;
				_tempColorMap = null;
            }

            // Lazy initialization of temporary objects

            if (_positionBuffer1 == null)
            {
                _positionBuffer1 = new ComputeBuffer(vcount_x3, sizeof(float));
                _positionBuffer2 = new ComputeBuffer(vcount_x3, sizeof(float));
                _normalBuffer = new ComputeBuffer(vcount_x3, sizeof(float));
				_colorBuffer = new ComputeBuffer(vcount_x3, sizeof(float));
			}

			if (_tempPositionMap == null)
            {
                _tempPositionMap = Utility.CreateRenderTexture(_positionMap);
                _tempVelocityMap = Utility.CreateRenderTexture(_positionMap);
                _tempNormalMap = Utility.CreateRenderTexture(_positionMap);
				_tempColorMap = Utility.CreateRenderTexture(_positionMap);
			}

			// Set data and execute the transfer task.

			_compute.SetInt("VertexCount", vcount);
            _compute.SetMatrix("Transform", sourceTransform.localToWorldMatrix);
            _compute.SetMatrix("OldTransform", _previousTransform);
            _compute.SetFloat("FrameRate", 1 / Time.deltaTime);
			_compute.SetFloat("MainTextureDimension", _textureMap.height);

            _positionBuffer1.SetData(_positionList);
            _normalBuffer.SetData(_normalList);
			_colorBuffer.SetData(_uvList);


			_compute.SetBuffer(0, "PositionBuffer", _positionBuffer1);
            _compute.SetBuffer(0, "OldPositionBuffer", _positionBuffer2);
            _compute.SetBuffer(0, "NormalBuffer", _normalBuffer);
			_compute.SetBuffer(0, "ColorBuffer", _colorBuffer);

			_compute.SetTexture(0, "PositionMap", _tempPositionMap);
            _compute.SetTexture(0, "VelocityMap", _tempVelocityMap);
            _compute.SetTexture(0, "NormalMap", _tempNormalMap);
			_compute.SetTexture(0, "ColorMap", _tempColorMap);
			_compute.SetTexture(0, "MainTex", _textureMap);

			_compute.Dispatch(0, mapWidth / 8, mapHeight / 8, 1);

            Graphics.CopyTexture(_tempPositionMap, _positionMap);
            Graphics.CopyTexture(_tempVelocityMap, _velocityMap);
            Graphics.CopyTexture(_tempNormalMap, _normalMap);
			Graphics.CopyTexture(_tempColorMap, _colorMap);
		}

		bool _warned;

        bool CheckConsistency()
        {
            if (_warned) return false;

            if (_positionMap.width % 8 != 0 || _positionMap.height % 8 != 0)
            {
                Debug.LogError("Position map dimensions should be a multiple of 8.");
                _warned = true;
            }

            if (_normalMap.width != _positionMap.width ||
                _normalMap.height != _positionMap.height)
            {
                Debug.LogError("Position/normal map dimensions should match.");
                _warned = true;
            }

            if (_positionMap.format != RenderTextureFormat.ARGBHalf &&
                _positionMap.format != RenderTextureFormat.ARGBFloat)
            {
                Debug.LogError("Position map format should be ARGBHalf or ARGBFloat.");
                _warned = true;
            }

            if (_normalMap.format != RenderTextureFormat.ARGBHalf &&
                _normalMap.format != RenderTextureFormat.ARGBFloat)
            {
                Debug.LogError("Normal map format should be ARGBHalf or ARGBFloat.");
                _warned = true;
            }

            return !_warned;
        }

        #endregion
    }
}
