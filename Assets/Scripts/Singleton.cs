using UnityEngine;
using System.Collections;

//class for instances that can be only one in the scene (manager scripts)
public class Singleton<T> : MonoBehaviour where T: MonoBehaviour {


	static T m_instance;


	public static T Instance
	{
		//find the T instance's type 
		get
		{
			if (m_instance == null)
			{
				m_instance = GameObject.FindObjectOfType<T>();

				if
					(m_instance == null)
				{
					GameObject singleton = new GameObject(typeof(T).Name);
					m_instance = singleton.AddComponent<T>();
				}
			}
			return m_instance;
		}
	}

	//"virtual" keyword used for inherited classes
	//guarantees that there is only one instance in the scene
	public virtual void Awake()
	{

		if (m_instance == null)
		{
			//casting
			m_instance = this as T;
			//preventing singleton object to be destroyed during other scenes are loaded
			//DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	
}
