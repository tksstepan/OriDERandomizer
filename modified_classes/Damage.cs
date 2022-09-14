using System;
using UnityEngine;

public class Damage
{
	public Damage(float amount, Vector2 force, Vector3 position, DamageType type, GameObject sender)
	{
		this.m_amount = amount;
		this.m_force = force;
		this.m_position = position;
		this.m_type = type;
		this.m_sender = sender;
		if (type == DamageType.SpiritFlame)
		{
			this.m_amount += (float)RandomizerBonus.SpiritFlameLevel();
		}
	}

	public float Amount
	{
		get
		{
			return this.m_amount;
		}
	}

	public Vector2 Force
	{
		get
		{
			return this.m_force;
		}
	}

	public Vector3 Position
	{
		get
		{
			return this.m_position;
		}
	}

	public DamageType Type
	{
		get
		{
			return this.m_type;
		}
	}

	public GameObject Sender
	{
		get
		{
			return this.m_sender;
		}
	}

	public void SetAmount(float amount)
	{
		this.m_amount = amount;
	}

	public void DealToComponents(GameObject target)
	{
		if (target != null)
		{
			target.SendMessage("OnRecieveDamage", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	private float m_amount;

	private Vector2 m_force;

	private Vector3 m_position;

	private DamageType m_type;

	private GameObject m_sender;
}
