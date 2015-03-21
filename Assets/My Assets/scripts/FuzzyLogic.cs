using UnityEngine;	
using System;

public class FuzzyLogic : MonoBehaviour{


	/*
	 * Calcula probabilidades difusas
	 * 	De entrada se requere una matriz de 3XN
	 * 	Donde cada fila representa un nodo con ( Comida, Rivales, Compañeros )
	 * 	@
	 */
	public double[] calculate( double[,] G ){
		//Evalua el grafo
		if (G.GetLength (0) != 3) {
			Debug.Log("[ERROR]:\nLa tabla de nodos solo puede tener 3 renglones.\n(Comida, Rivales y Compañeros).");
			double[] error = new double[]{};
			return error;
		}

		//Prepara variables
		double[] ps = new double[G.GetLength(1)];
		double comidaTotal, rivalesTotal, companerosTotal;
		//double comidaPertenencia, rivalesPertenencia, companerosPertenencia;
		double comida, rivales, companeros;
		int grafoLength = G.GetLength(1);

		int i, j;
		//Columnas del grafo
		int C_Comida    = 0;
		int C_Rivales   = 1;
		int C_Comaneros = 2;
		//Valores para defusificar
		int defusificaBajo  = 60; // 0+10+20+30   = 60
		int defusificaMedio = 150;// 40+50+60     = 150
		int defusificaAlto  = 340;// 70+80+90+100 = 340
		double[] defusificaCentroide = new double[]{0.43F,1F,0.43F,0.685F,1F,0.685F,0.5F,1F,0.5F,0F};

		//Para calcular el centroide
		double centroideDivisor = 0;
		double dividendo;

		//Cuenta los valores totales
		comidaTotal = rivalesTotal = companerosTotal = 0;

		//consola.Buffer.Text += "len("+Convert.ToString(grafoLength)+")\n";

		for ( i = 0; i < grafoLength; i++) {//Para cada nodo
			comidaTotal     = comidaTotal + G[C_Comida,i];
			rivalesTotal    = rivalesTotal+ G[C_Rivales,i];
			companerosTotal = companerosTotal + G[C_Comaneros,i];
			//consola.Buffer.Text += "comida("+Convert.ToString(G[C_Comida,i])+") rivales("+Convert.ToString(G[C_Rivales,i])+") compañeros("+Convert.ToString(G[C_Comaneros,i])+")\n";
		}
		//consola.Buffer.Text += "comida("+Convert.ToString(comidaTotal)+") rivales("+Convert.ToString(rivalesTotal)+") compañeros("+Convert.ToString(companerosTotal)+")\n";

		//Para cada nodo
		for( i = 0; i < grafoLength; i++ ){
			
			//Obtiene valor ponderado de las variables de interes
			comida     = ( (G[C_Comida,i]*100)/comidaTotal ) / 100;
			rivales    = ( (G[C_Rivales,i]*100)/rivalesTotal ) / 100;
			companeros = ( (G[C_Comaneros,i]*100)/companerosTotal ) / 100;
			
			//Aplica las reglas difusas
			// 1) Poca comida v Muchos rivales v Muchos compañeros.
			// 2) Comida regular v rivales regular v compañeros regular.
			// 3) 
			//		a) Mucha comida & ( Pocos Rivales v Pocos Compañeros v (Muchos rivales & Medios Compañeros) )
			//		b) Mucha comida & Pocos rivales & pocos compañeros
			
			// 1)
			double tmpRegla1 = Math.Max ( comidaBajo(comida), rivalesAlto(rivales) );
			tmpRegla1 = Math.Max ( tmpRegla1, companerosAlto(companeros) );
			
			// 2)
			double tmpRegla2 = Math.Max ( comidaMedio(comida), rivalesMedio(rivales) );
			tmpRegla2 = Math.Min ( tmpRegla2, companerosMedio(companeros) );
			
			// 3)
			double tmpRegla3 = Math.Min ( rivalesAlto(rivales), companerosMedio(companeros) );
			tmpRegla3 = Math.Max ( tmpRegla3, companerosBajo(companeros) );
			tmpRegla3 = Math.Max ( tmpRegla3, rivalesBajo(rivales) );
			tmpRegla3 = Math.Min ( tmpRegla3, comidaAlto(comida) );

			
			//Obtiene el factor sobre el que se dividira en el centroide
			centroideDivisor = 0;
			for( j = 0; j < defusificaCentroide.Length; j++ ){//Para cada nodo
				switch (j) {
				case 0:
				case 1:
				case 2:
				case 3:
					centroideDivisor += Math.Min (tmpRegla1, defusificaCentroide [j]);
					/*if (debugeando) {
						consola.Buffer.Text += "+ " + Convert.ToString (Math.Min (tmpRegla1, defusificaCentroide [j])) + "\n";
					}*/
					break;
				case 4:
				case 5:
				case 6:
				case 7:
					centroideDivisor += Math.Min (tmpRegla2, defusificaCentroide [j]);
					/*if (debugeando) {
						consola.Buffer.Text += "+ " + Convert.ToString (Math.Min (tmpRegla2, defusificaCentroide [j])) + "\n";
					}*/
					break;
				case 8:
				case 9:
				case 10:
					centroideDivisor += Math.Min (tmpRegla3, defusificaCentroide [j]);
					/*if (debugeando) {
						consola.Buffer.Text += "+ " + Convert.ToString (Math.Min (tmpRegla3, defusificaCentroide [j])) + "\n";
					}*/
					break;
				}
			}

			//Guarda la probabilidad como centroide en la variable que entregara
			dividendo = (defusificaBajo * tmpRegla1) + (defusificaMedio * tmpRegla2) + (defusificaAlto * tmpRegla3);
			
			if (dividendo == 0) {
				ps [i] = 0F;
			} else {
				ps [i] = ((defusificaBajo * tmpRegla1) + (defusificaMedio * tmpRegla2) + (defusificaAlto * tmpRegla3)) / centroideDivisor;
			}
			ps [i] = Math.Round (ps [i], 2);
		}

		//Pondera los totales
		if( ps.GetLength(0) > 0 ){
			//Obtiene el total de valores difusos
			double ProbTotal = 0;
			for( i=0; i < G.GetLength(1); i++  ){
				ProbTotal += ps [i];
			}
			//Pone los valores ponderados
			for( i=0; i < G.GetLength(1); i++  ){
				ps [i] = Math.Round ( ( (ps [i] * 100) / ProbTotal), 2 );
			}
		}

		return ps;
	}


	//COMIDA
	protected double comidaBajo( double x ){
		int y = (int)( Math.Round(x,1) * 10 );
		double[] fit = new double[] {0,.3,.6,.8,1,1,1,1,1,1,1};
		//double[] fit = new double[] {0,0,0,0,0,0,0,.23,.5,.75,1};
		//double[] fit = new double[] {.1,.3,.5,.7,.9,1,1,1,1,1,1};
		return fit[y];
	}
	protected double comidaMedio( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		double[] fit = new double[] {0,0,0,.4,.7,1,.7,.4,0,0,0};
		//double[] fit = new double[] {0,0,0,0,.28,.55,.8,1,1,1,1};
		//double[] fit = new double[] {.1,.3,.5,.7,.9,1,1,1,1,1,1};
		return fit[y];
	}
	protected double comidaAlto( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		//double[] fit = new double[] { 0, 0, 0, 0, .6, 0.8, 1, 0.8, 0.6, 0.3, 0.1};
		double[] fit = new double[] {0,0,0,0,0,0,.2,.4,.6,.8,1};
		//double[] fit = new double[] {0,.4,.7,1,1,1,1,1,1,1,1};
		//double[] fit = new double[] {.1,.3,.5,.7,.9,1,1,1,1,1,1};
		return fit[y];
	}
	
	//RIVALES
	protected double rivalesBajo( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		//double[] fit = new double[] {.6,.6,.48,.35,.22,.1,0,0,0,0,0};
		double[] fit = new double[] {0,.3,.6,.8,1,1,1,1,1,1,1};
		return fit[y];
	}
	protected double rivalesMedio( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		//double[] fit = new double[] {1,.9,.8,.7,.6,.5,.4,.3,.2,.1,0};
		double[] fit = new double[] {0,0,0,.4,.7,1,.7,.4,0,0,0};
		return fit[y];
	}
	protected double rivalesAlto( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		//double[] fit = new double[] {1,1,1,1,.9,.8,.72,.63,.52,.45,.36};
		double[] fit = new double[] {0,0,0,0,0,0,.2,.4,.6,.8,1};
		return fit[y];
	}
	
	//COMPAÑEROS
	protected double companerosBajo( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		//double[] fit = new double[] {.6,.88,1,.88,.7,.5,.3,0,0,0,0};
		double[] fit = new double[] {0,.3,.6,.8,1,1,1,1,1,1,1};
		return fit[y];
	}
	protected double companerosMedio( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		//double[] fit = new double[] {0,.2,.4,.6,.8,1,.8,.6,.4,.2,0};
		double[] fit = new double[] {0,0,0,.4,.7,1,.7,.4,0,0,0};
		return fit[y];
	}
	protected double companerosAlto( double x ){
		int y = (int)( Math.Round(x,1) * 10 ); 
		//double[] fit = new double[] {0,0,0,0,.2,.4,.6,.8,1,.7,.5};
		double[] fit = new double[] {0,0,0,0,0,0,.2,.4,.6,.8,1};
		return fit[y];
	}
}
