using UnityEngine;
using System.Collections;

public class PreyLeaderChoosing : MonoBehaviour
{

    public float leadership;
    private bool requestResponded;
    private GameObject tempLeader;
    private int comRange;

    // Llamada a la votacion
    public void choose()
    {
        leadership = GetComponent<Prey>().getLeadershipStat();
        comRange = GetComponent<Prey>().comRange;

        StartCoroutine(startElection());
        StartCoroutine(endElection());
    }

    /// <summary>
    /// Esperar un tiempo antes de empezar eleccion
    /// </summary>
    private IEnumerator startElection()
    {
        yield return new WaitForSeconds(.5f);
        sendElectionMessage();
    }


    /// <summary>
    /// La eleccion termino, enviar quien sera el lider
    /// </summary>
    private IEnumerator endElection()
    {
        yield return new WaitForSeconds(3);
        if (requestResponded == false)
        {
            BroadcastLeadership(gameObject);

            //Espera 2 segundos por si alguien tambien quiere ser lider y tiene mejores capacidades que yo
            yield return new WaitForSeconds(2);
            if (tempLeader == null)
                tempLeader = gameObject;
            GetComponent<Prey>().setLeader(tempLeader);
            if (tempLeader.GetInstanceID() == gameObject.GetInstanceID() && !requestResponded)
                becomeLeader();
        }
        else
        {
            yield return new WaitForSeconds(2);
            GetComponent<Prey>().setLeader(tempLeader);
        }
    }

    /**
	 * Les solicita a los que tienen mejor capacidad de liderazgo que si pueden ser lideres
	 **/

    private void sendElectionMessage()
    {

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
        //Por cada objeto encontrado
        for (int i = 0; i < hitColliders.Length; i++)
        {

            //Si es un velocirraptor
            if (hitColliders[i].GetComponent<Prey>() != null)
            {
                //Que no soy yo
                if (hitColliders[i].gameObject.GetInstanceID() != gameObject.GetInstanceID())
                {
                    //Si es mejor lider que yo
                    if (leadership < hitColliders[i].gameObject.GetComponent<PreyLeaderChoosing>().leadership)
                    {
                        //Pidele que sea lider
                        hitColliders[i].SendMessage("leadershipRequest", gameObject);
                    }
                }
            }
        }
    }

    /**
     * Consegui ser lider, crea la luz encima de el
     **/

    private void becomeLeader()
    {
        //Crea el objeto al que se le agregara la luz
        GameObject brigth = new GameObject("leaderLigth");
        brigth.AddComponent(typeof (Light)); //se le agrega la luz

        brigth.transform.parent = transform; //Se fija a la entidad


        brigth.light.type = LightType.Spot; //Se elije el tipo de luz SPOT

        //Se pone la mira hacia abajo
        brigth.transform.position = brigth.transform.parent.position + new Vector3(0, 1, 0);
        brigth.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));

        //Color, Alcance, Dispercion
        brigth.light.color = Color.white;
        brigth.light.range = 12.2f;
        brigth.light.spotAngle = 51.2f;

    }



    /**
     * Informar quien sera el lider
     **/

    private void BroadcastLeadership(GameObject leader)
    {
        if (tempLeader != null && tempLeader.GetInstanceID() == leader.GetInstanceID())
        {
            return;
        }

        if (tempLeader == null ||
            tempLeader.GetComponent<PreyLeaderChoosing>().leadership <
            leader.GetComponent<PreyLeaderChoosing>().leadership)
        {
            tempLeader = leader;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
            //Por cada objeto encontrado
            for (int i = 0; i < hitColliders.Length; i++)
            {

                //Si es un velocirraptor
                if (hitColliders[i].GetComponent<Prey>() != null)
                {

                    //Que no soy yo
                    if (hitColliders[i].gameObject.GetInstanceID() != gameObject.GetInstanceID())

                        //Enviale la eleccion de lider
                        hitColliders[i].SendMessage("BroadcastLeadership", tempLeader);

                }
            }
        }
    }


    /**
	 * Me solicitan ser lider
	 **/

    private void leadershipRequest(GameObject sender)
    {
        sender.SendMessage("leadershioRequestResponse");
    }



    /**
	 * Respuesta a la solicitud de lideresgo
	 **/

    private void leadershioRequestResponse()
    {
        //Alguien acepto el cargo, no puedo ser yo
        requestResponded = true;
    }
}