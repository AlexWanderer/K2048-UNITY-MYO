﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

// state for status through Coroutine delay
public enum GameState{
	Playing,
	GameOver,
	WaitingForMove
}

public class GameManager : MonoBehaviour {

	// NEW AFTER ADDED DELAYS
	public GameState State;
	[Range(0, 2f)] // generate a slider for range 0 - 2f of delay variable
	public float delay;
	// local variable, related delay
	private bool[] lineMoveComplete = new bool[4]{true, true, true, true};
	// ...........................

	// declare at top, used for delay and the bug (do not add new tail if no move made)
	private bool MoveMade; 

	// game won object, use type GameObject, then can display them in hierarchy
	public GameObject GameWonText;
	// game over related elements
	public GameObject GameOverText;
	public Text GameOverScoreText;
	public GameObject GameOverPanel; 

	// store tiles (in Start method, the array will be deleted after executed)
	private Tile[,] AllTiles = new Tile[4, 4];
	// add tile array lists to store rows and columns
	private List<Tile[]> rows = new List<Tile[]> ();
	private List<Tile[]> columns = new List<Tile[]> ();
	// use list to store empty tiles
	private List<Tile> EmptyTiles = new List<Tile>();


	// Use this for initialization
	void Start () {
		// get all tiles, store in an array
		Tile[] AllTilesOneDim = GameObject.FindObjectsOfType<Tile>();
		foreach (Tile t in AllTilesOneDim) {
			// clear all tiles
			t.Number = 0;
			// add tile to alltiles array
			AllTiles[t.indRow, t.indCol] = t;
			// add to empty tile list
			EmptyTiles.Add(t);
		}

		// add rows and columns to tile array list
		rows.Add(new Tile[]{AllTiles[0,0],AllTiles[0,1],AllTiles[0,2],AllTiles[0,3]});
		rows.Add(new Tile[]{AllTiles[1,0],AllTiles[1,1],AllTiles[1,2],AllTiles[1,3]});
		rows.Add(new Tile[]{AllTiles[2,0],AllTiles[2,1],AllTiles[2,2],AllTiles[2,3]});
		rows.Add(new Tile[]{AllTiles[3,0],AllTiles[3,1],AllTiles[3,2],AllTiles[3,3]});
		columns.Add(new Tile[]{AllTiles[0,0],AllTiles[1,0],AllTiles[2,0],AllTiles[3,0]});
		columns.Add(new Tile[]{AllTiles[0,1],AllTiles[1,1],AllTiles[2,1],AllTiles[3,1]});
		columns.Add(new Tile[]{AllTiles[0,2],AllTiles[1,2],AllTiles[2,2],AllTiles[3,2]});
		columns.Add(new Tile[]{AllTiles[0,3],AllTiles[1,3],AllTiles[2,3],AllTiles[3,3]});

		// automatically generate two tiles when game start
		Generate();
		Generate();
	}

	// handle game won
	private void YouWon(){
		// set message to 'you won'
		GameOverText.SetActive (false);
		GameWonText.SetActive (true);
		// get current score
		GameOverScoreText.text = ScoreChecker.Instance.Score.ToString ();
		// display the GameOverPanel
		GameOverPanel.SetActive (true);
	}

	// handle game over 
	private void GameOver(){
		// get current score
		GameOverScoreText.text = ScoreChecker.Instance.Score.ToString ();
		// display the GameOverPanel
		GameOverPanel.SetActive (true);
	}

	// check the game is/not over, with no chance to move
	bool CanMove(){
		// if there exist empty tiles, means can move
		if (EmptyTiles.Count > 0)
			return true;
		else {
			// check there exist tiles can merging
			// check rows
			for (int i = 0; i < rows.Count; i++)
				for (int j = 0; j < columns.Count - 1; j++)
					if (AllTiles [i, j].Number == AllTiles [i, j + 1].Number)
						return true;
			// check columns
			for (int i = 0; i < columns.Count; i++)
				for (int j = 0; j < rows.Count - 1; j++)
					if (AllTiles [j, i].Number == AllTiles [j + 1, i].Number)
						return true;
		}
		return false; 
	}

	// start a new game
	public void NewGameButtonHandler(){
		// disable current myo hub,
		ThalmicHub hub = ThalmicHub.instance;
		DestroyImmediate(hub);
		// restart the scene
		Application.LoadLevel (Application.loadedLevel);
	}

	/* there are two type moves:
	 * 1) move right and down --> up index --> move index increase
	 * 2) move left and up --> down index --> move index decrease
	 */
	// the method of tile shifting in down index, return T/F
	bool MakeOneMoveDownIndex(Tile[] LineOfTiles){
		// for each line tile position, first check moving, then merge
		for (int i = 0; i < LineOfTiles.Length - 1; i++) {
			// move block, if current tile is empty & next is not empty, then move
			if(LineOfTiles [i].Number == 0 && LineOfTiles[i + 1].Number != 0){
				LineOfTiles [i].Number = LineOfTiles [i + 1].Number;
				LineOfTiles [i + 1].Number = 0;
				return true;
			}
			// merge block
			// check two tiles have same number ot is/not merged once
			if (LineOfTiles [i].Number != 0 &&
				LineOfTiles [i].Number == LineOfTiles [i + 1].Number &&
				LineOfTiles [i].MergeThisTurn == false &&
				LineOfTiles [i+1].MergeThisTurn == false) {
				LineOfTiles [i].Number *= 2;
				LineOfTiles [i + 1].Number = 0;
				LineOfTiles [i].MergeThisTurn = true;

				// set merge animation to new tile
				LineOfTiles[i].PlayMergeAnimation();

				// tracking score
				ScoreChecker.Instance.Score += LineOfTiles[i].Number;

				// check game is/not won
				if (LineOfTiles [i].Number == 2048)
					YouWon ();
				return true;
			}
		}
		return false;
	}
	// the method of tile shifting in up index, return T/F
	bool MakeOneMoveUpIndex(Tile[] LineOfTiles){
		// for each line tile position, first check moving, then merge
		for (int i = LineOfTiles.Length - 1; i > 0; i--) {
			// move block, if current tile is empty & next is not empty, then move
			if(LineOfTiles [i].Number == 0 && LineOfTiles[i - 1].Number != 0){
				LineOfTiles [i].Number = LineOfTiles [i - 1].Number;
				LineOfTiles [i - 1].Number = 0;
				return true;
			}
			// merge block
			// check two tiles have same number ot is/not merged once
			if (LineOfTiles [i].Number != 0 &&
				LineOfTiles [i].Number == LineOfTiles [i - 1].Number &&
				LineOfTiles [i].MergeThisTurn == false &&
				LineOfTiles [i - 1].MergeThisTurn == false) {
				LineOfTiles [i].Number *= 2;
				LineOfTiles [i - 1].Number = 0;
				LineOfTiles [i].MergeThisTurn = true;

				// set merge animation to new tile
				LineOfTiles[i].PlayMergeAnimation();

				// tracking score
				ScoreChecker.Instance.Score += LineOfTiles[i].Number; 

				// check game is/not won
				if (LineOfTiles [i].Number == 2048)
					YouWon ();
				return true;
			}
		}
		return false;
	}


	// generate a new tile (with number 2/4)
	void Generate(){
		// if empty tile exists, then generate a tile (2/4) in random position.
		if (EmptyTiles.Count > 0) {
			int indexForNewNumber = Random.Range (0, EmptyTiles.Count);
			// create a random number to control generate 4
			int RandomNumber = Random.Range(0,10);
			if(RandomNumber == 0)
				EmptyTiles [indexForNewNumber].Number = 4;
			else
				EmptyTiles [indexForNewNumber].Number = 2;

			// set appear animation to new tile
			EmptyTiles[indexForNewNumber].PlayAppearAnimation();

			EmptyTiles.RemoveAt (indexForNewNumber);
		}
	}
	
//	// Update is called once per frame
//	void Update () {
//		// add event to test method 'Generate'
//		if (Input.GetKeyDown (KeyCode.G)) {
//			Generate ();
//		}
//	}

	// after each move, we need to update the Empty Tiles list
	// otherwise, we can only add 16 new tiles...
	private void UpdateEmptyTiles(){
		// step 1, clear the list
		EmptyTiles.Clear();
		// step 2, loop each element, to add empty tiles to the list
		foreach(Tile t in AllTiles){
			if (t.Number == 0)
				EmptyTiles.Add (t);
		}
	}

	// the method to reset all tiles' merge status
	public void ResetMergedFlags(){
		foreach (Tile t in AllTiles) {
			t.MergeThisTurn = false;
		}
	}

	public void Move (MoveDirection md)
	{
		Debug.Log (md.ToString () + " move.");

		// there is a sinatio: if no moves and merges when trigger a direction
		// it should not generate a new tile.
		// MoveMade is to aviod this bug.
		MoveMade = false;

		// reset merged flag to false
		ResetMergedFlags();

		// check delay is/not set
		if (delay > 0)
			StartCoroutine (MoveCoroutine (md));
		else {
			// create game logic.
			// there are 4 rows and 4 columns
			// so, for each line should affect
			for(int i = 0; i < rows.Count; i++){
				// switch four direction moves
				switch (md) {
				case MoveDirection.Down: 
					// shift tiles, which means if there exist empty tile, then shift
					// correct tile to that position.
					while(MakeOneMoveUpIndex(columns[i])) {
						// set MoveMade = true
						MoveMade = true;
					}
					break;
				case MoveDirection.Left: 
					while(MakeOneMoveDownIndex(rows[i])) {
						MoveMade = true;
					}
					break;
				case MoveDirection.Right: 
					while(MakeOneMoveUpIndex(rows[i])) {
						MoveMade = true;
					}
					break;
				case MoveDirection.Up: 
					while(MakeOneMoveDownIndex(columns[i])) {
						MoveMade = true;
					}
					break;
				}
			}

			// ckeck the MoveMade value, avoid generating a new tile when there have no moves
			if (MoveMade) {
				// update empty tile list
				UpdateEmptyTiles();
				// add a new tile after the move finished
				Generate (); 

				// check game is/not over
				if(!CanMove()){
					GameOver ();
				}
			}
		}
	}

	/* these are coroutine for delay between current move and next move.
	 * show user the moving track
	 */
	IEnumerator MoveCoroutine(MoveDirection md)
	{
		State = GameState.WaitingForMove;

		// start moving each line with delays depending on MoveDirection md
		switch (md) 
		{
		case MoveDirection.Down:
			for (int i = 0; i< columns.Count; i++)
				StartCoroutine(MoveOneLineUpIndexCoroutine(columns[i], i));
			break;
		case MoveDirection.Left: 
			for (int i = 0; i< rows.Count; i++)
				StartCoroutine(MoveOneLineDownIndexCoroutine(rows[i], i));
			break;
		case MoveDirection.Right:
			for (int i = 0; i< rows.Count; i++)
				StartCoroutine(MoveOneLineUpIndexCoroutine(rows[i], i));
			break;
		case MoveDirection.Up: 
			for (int i = 0; i< columns.Count; i++)
				StartCoroutine(MoveOneLineDownIndexCoroutine(columns[i], i));
			break;

		}

		// Wait until the move is over in all lines
		while (! (lineMoveComplete[0] && lineMoveComplete[1] && lineMoveComplete[2] && lineMoveComplete[3]))
			yield return null;

		if (MoveMade) 
		{
			UpdateEmptyTiles ();
			Generate ();

			if (!CanMove())
			{
				GameOver();
			}

		}

		State = GameState.Playing;
		StopAllCoroutines ();
	}

	IEnumerator MoveOneLineUpIndexCoroutine(Tile[] line, int index)
	{
		lineMoveComplete [index] = false;
		while (MakeOneMoveUpIndex(line)) 
		{
			MoveMade = true;
			// delay next step
			yield return new WaitForSeconds(delay);
		}
		lineMoveComplete [index] = true;
	}

	IEnumerator MoveOneLineDownIndexCoroutine(Tile[] line, int index)
	{
		lineMoveComplete [index] = false;
		while (MakeOneMoveDownIndex(line)) 
		{
			MoveMade = true;
			// delay next step
			yield return new WaitForSeconds(delay);
		}
		lineMoveComplete [index] = true;
	}
}
