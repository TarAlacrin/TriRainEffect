uniform StructuredBuffer<float3> _Compy;

void PassThru_float(float inID, out float3 flOut)
{
	_Compy[inID] = 1;
	flOut = float3(0, 0, 1);
	//flOut = _Compy[0];
}