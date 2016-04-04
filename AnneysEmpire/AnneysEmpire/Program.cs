﻿using AnneysEmpire.Learning;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnneysEmpire
{
	/// <summary>
	/// This program is designed to take in input from spreadsheet or
	/// database system, and generate AI code for Age of Empires 2: Age of Kings.
	/// It uses an Artificial Nueral Network to take in game scores and wieghs each
	/// category against existing code blocks in order to decide which values
	/// allow the AI to manage their base in a more efficient way such that they 
	/// can defeat either another AI or another player.
	/// 
	/// NOTE: As proof of concept, we may need to generate more deterministic models for this,
	///  like simulating a game, and then running it to get better test results.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// The main execution method.
		/// </summary>
		/// <param name="args">The arguments.</param>
		public static void Main(string[] args)
		{
			// This is how I like my consoles.
			Console.BackgroundColor = ConsoleColor.DarkBlue;
			Console.Clear();

			//To do write unit test for my Matrix/Vector library

			//TwoLayer.NeuralNetowrk();
			//TwoLayerInternal.NeuralNetowrkWithAnnMath();
			//ThreeLayerNueralNet.RunNet();
			TextWriter tmp = Console.Out;

            //using (StreamWriter writer = new StreamWriter("out.txt"))
            //{
            //    Console.SetOut(writer);
            //    var ag = new TestingNdn(writer);
            //}

            // Default constructor for whatever stuff.
            //var ag = new TestNdn2(5, 10, 1);

            AiTrainingModule.Test("C:\\AnnAoe2\\AnneysEmpire\\AnneysEmpire\\Data\\data.csv",5,10,4);



			Console.SetOut(tmp);
			Console.ReadKey();
		}
	}
}
