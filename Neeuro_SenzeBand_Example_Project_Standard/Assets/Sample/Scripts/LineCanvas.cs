using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LineCanvas : MonoBehaviour
{
    public Vector3 Min;
    public Vector3 Max;

    public int WindowSize = 0;

    public float ScrollSpeed = 0.0f;
    public LineRenderer Line;

    public float ClampedMagnitude = 1;
    class ScrollingBuffer
    {
        //public static float ScrollSpeed = -5.0f;
        public List<Vector3> data = new List<Vector3>();
        public float origin_index = 0.0f;
    }

    private string PrintBuffers()
    {
        string result = "buffers.Count:" + buffers.Count + "\n";
        for (int i = 0; i < buffers.Count; ++i)
        {
            var buffer = buffers[i];
            result += "Buffer_" + i + "(size:" + buffer.data.Count + ", origin_index=" + buffer.origin_index + ")\n";
        }
        return result;

    }

    public void SetData(int[] data)
    {
        float[] d = new float[data.Length];
        for(int i = 0; i < data.Length; ++i)
        {
            d[i] = (float)data[i];
        }

        SetData(d);        
    }

    List<ScrollingBuffer> buffers = new List<ScrollingBuffer>();
    public void SetData(float[] data)
    {
        Debug.Log("LineCanvas SetData Start");

        var line_data = GetPoints(data);

        ScrollingBuffer sbuffer = new ScrollingBuffer();
        sbuffer.data = line_data;

        if (buffers.Count > 0)
        {
            var last_buffer = buffers[buffers.Count - 1];

            sbuffer.origin_index = last_buffer.origin_index + last_buffer.data.Count;
        }
        else
            sbuffer.origin_index = WindowSize;

        buffers.Add(sbuffer);

        Debug.Log("LineCanvas SetData End");

        //Debug.Log("SetData");
        //Debug.Log(PrintBuffers());
    }

    List<Vector3> GetPoints(float[] data)
    {
        var line_data = new List<Vector3>(WindowSize);        

        var local_to_world = transform.localToWorldMatrix;
        float xdelta = 1.0f / WindowSize;

        Vector3 minmaxdelta = Max - Min;
        
        if (data != null)
        {
            var normalised_data = Normalize(data);

            int i = 0;
            foreach (var p in normalised_data)
            {
                float xoffset = xdelta * i;

                var localpoint = new Vector3(Min.x + (xoffset * minmaxdelta.x), 0, Min.z + minmaxdelta.z * p);

                line_data.Add(localpoint);

                ++i;
            }
        }
        else
        {
            for(int i = 0; i < WindowSize; i++)
            {
                float xoffset = xdelta * i;

                var localpoint = new Vector3(Min.x + (xoffset * minmaxdelta.x), 0, Min.z + minmaxdelta.z * .5f);

                line_data.Add(localpoint);
                
            }
        }

        return line_data;
    }

    public void ClearBuffers()
    {
        buffers.Clear();
    }    

    private void Update()
    {
        //Debug.Log("LineCanvas Update Start");
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();

        var line_data = GetPoints(null);

        if(ScrollSpeed > 0.0f) ScrollSpeed *= -1.0f;

        List<ScrollingBuffer> expired_buffers = new List<ScrollingBuffer>();
        
        foreach (var buffer in buffers)
        {
            if(buffer.origin_index + buffer.data.Count < 0.0f)
            {
                expired_buffers.Add(buffer);
                continue;
            }

            int i = 0;

            //string positions = "";

            //Debug.Log("line_data.Count=" + line_data.Count);

            foreach (var point in buffer.data)
            {
                float position = buffer.origin_index + i;
                int index = Mathf.FloorToInt(position);                

                if (index < line_data.Count && index > 0)
                {                    
                    line_data[index] = new Vector3(line_data[index].x, 0.0f, point.z);
                    //positions += "[" + index + "=" + point.z + "]";
                }

                ++i;
            }
            //Debug.Log("buffer.origin_index=" + buffer.origin_index + ": " + positions);

            buffer.origin_index += ScrollSpeed * Time.deltaTime;            
        }
        stopWatch.Stop();
        //Debug.Log("LineCanvas Update Buffer - " + stopWatch.Elapsed.TotalMilliseconds);


        foreach (var buffer in expired_buffers)
            buffers.Remove(buffer);

        Line.useWorldSpace = false;
        Line.positionCount = WindowSize;
        Line.SetPositions(line_data.ToArray());

        //Debug.Log("LineCanvas Update End");
    }

    float max_magnitude = float.MinValue;
    public bool absolute_range = true;
    List<float> Normalize(float[] data)
    {
        if (data == null || data.Length < 1) return new List<float>(data);

        List<float> ret = new List<float>();

        if(ClampedMagnitude == 0.0f) ClampedMagnitude = 1.0f;

        ClampedMagnitude = Mathf.Abs(ClampedMagnitude);

        bool negatives = false;
        float current_max = float.MinValue;
        foreach (var d in data)
        {
            float m = Mathf.Abs(d);

            if (m > current_max) current_max = m;

            if (negatives == false && d < 0.0f) negatives = true;
        }

        if(current_max < max_magnitude)
            max_magnitude = (current_max + max_magnitude) / 2;
        else
            max_magnitude = current_max;

        if (max_magnitude == 0.0f) // do not divide by 0
            return new List<float>(data);

        if(!absolute_range)
            max_magnitude = Mathf.Clamp(max_magnitude, 0, ClampedMagnitude);
        else
            max_magnitude = ClampedMagnitude;

        if(max_magnitude == 0.0f)
            max_magnitude = ClampedMagnitude;

        //Debug.Log("max_magnitude = " + max_magnitude + " current_max = " + current_max);

        foreach (var d in data)
        {
            float val = Mathf.Clamp(d, -ClampedMagnitude, ClampedMagnitude) / max_magnitude;

            //Debug.Log("d = " + d + " val = " + val);

            if (negatives == false)
                ret.Add(val);
            else
                ret.Add((val + 1.0f) / 2.0f);
        }

        return ret;
    }
}