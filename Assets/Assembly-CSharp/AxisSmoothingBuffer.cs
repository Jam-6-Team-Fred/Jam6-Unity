public class AxisSmoothingBuffer
{
	public const int k_bufferSize = 10;

	public const float k_weightOfLatestValue = 1f;

	public const float k_weightFalloff = 0.5f;

	private float[] _buffer;

	private int _bufferIndex;

	public AxisSmoothingBuffer()
	{
		_bufferIndex = -1;
		_buffer = new float[10];
	}

	public void Update(float value)
	{
		_bufferIndex++;
		if (_bufferIndex > 9)
		{
			_bufferIndex = 0;
		}
		_buffer[_bufferIndex] = value;
	}

	public float GetAverage()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 1f;
		int num4 = _bufferIndex;
		do
		{
			num += _buffer[num4] * num3;
			num2 += num3;
			num3 *= 0.5f;
			num4--;
			if (num4 < 0)
			{
				num4 = _buffer.Length - 1;
			}
		}
		while (num4 != _bufferIndex);
		return num / num2;
	}
}
