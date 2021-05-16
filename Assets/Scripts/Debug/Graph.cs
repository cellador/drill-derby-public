using UnityEngine;

/// <summary>
/// Will display internal values visually on screen as a graph.
/// Drop as a component of an empty game object. 
/// </summary>
public class Graph : MonoBehaviour
{	
	public enum trackValue
	{
        turn,
		angMom,
        velocity,
        xAxis,
		yAxis,
		xvelocity,
		yvelocity
	}
	
	public bool averageMode = false;
	public trackValue trackedValueType;
	public int graphPoints = 10;
	public float lineWidth = 0.5f;
    public float distanceFromCamera = 2F;
	public Material lineMaterial;
	public Color lineColor;
	public Vector2 scale, offset;
	
	float lastValue;
	float[] data;
	float average;
	GameObject trackedcharacter;
	
	LineRenderer lineRenderer;

	void Start()
	{
		gameObject.layer = LayerMask.NameToLayer("UI");
		lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineMaterial.color = lineColor;
        lineRenderer.material = lineMaterial;
        lineRenderer.enabled = true;
        lineRenderer.positionCount = graphPoints;
		
		data = new float[graphPoints];
		lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
	}

	void FixedUpdate()
	{
		if(!trackedcharacter)
			trackedcharacter = GameManager.Instance.Players[0].gameObject;
	
		average = 0;
		for (int i = 0; i < graphPoints-1; i++)
		{
			average += data[i];
			data[i] = data[i + 1];
		}
		
		float newData;
		
		switch (trackedValueType)
        {
            case trackValue.velocity:
                newData = trackedcharacter.GetComponent<DrillCharacterController>().Velocity.magnitude;
                break;
            case trackValue.angMom:
                newData = trackedcharacter.GetComponent<DrillCharacterController>().AngMom*0.02f;
                break;
            case trackValue.turn:
                newData = trackedcharacter.GetComponent<DrillCharacterController>().input_turn;
                break;
            case trackValue.yAxis:
			    newData = trackedcharacter.transform.position.y;
			    break;
		    case trackValue.xAxis:
			    newData = trackedcharacter.transform.position.x;
			    break;
		    case trackValue.xvelocity:
			    newData = lastValue - trackedcharacter.transform.position.x;
			    lastValue = trackedcharacter.transform.position.x;
			    break;
		    case trackValue.yvelocity:
			    newData = -(lastValue - trackedcharacter.transform.position.y);
			    lastValue = trackedcharacter.transform.position.y;
			    break;
		    default:
			    newData = 0;
			    break;
		}
		
		data[graphPoints - 1] = newData;
		
		average = average / graphPoints;
		
		for (int i = 0; i < graphPoints; i++)
		{
			if (averageMode)
			{
				lineRenderer.SetPosition(i, new Vector3(i * scale.x + offset.x, (data[i] - average) * scale.y + offset.y, distanceFromCamera));
			}
			else
			{
				lineRenderer.SetPosition(i, new Vector3(i * scale.x + offset.x, (data[i]) * scale.y + offset.y, distanceFromCamera));
			}
		}
	}
}