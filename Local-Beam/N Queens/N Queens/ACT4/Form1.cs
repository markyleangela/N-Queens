using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;

namespace ACT4
{
    public partial class Form1 : Form
    {
        Random r = new Random();
        int side;
        int n = 6;
        int k = 3;
        SixState startState;
        List<SixState> currentState;
        int moveCounter;

        //bool stepMove = true;

        List<List<Point>> bMoves;
        List<Point> chosenMove;

        public Form1()
        {
            InitializeComponent();

            side = pictureBox1.Width / n;

            currentState = new List<SixState>();
            //for(int i = 0; i < k; i++) currentState.Add(randomSixState());
            currentState.Add(randomSixState());
            currentState.Add(randomSixState());
            currentState.Add(randomSixState());
            startState = currentState[0];

            updateUI();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private SixState randomSixState()
        {
            //SixState random = new SixState(r.Next(n),
            //                                 r.Next(n),
            //                                 r.Next(n),
            //                                 r.Next(n),
            //                                 r.Next(n),
            //                                 r.Next(n));
            //SixState random = new SixState(5, 5, 5, 5, 0, 1);
            //SixState random = new SixState(5, 3, 3, 4, 4, 3);
            SixState random = new SixState(2, 0, 4, 4, 5, 3);
            return random;
        }

        private void updateUI()
        {
            //bMoves.Clear();
            //pictureBox1.Refresh();
            pictureBox2.Refresh();

            //label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
            label3.Text = "Attacking pairs: " + getAttackingPairs(currentState[0]);
            label4.Text = "Moves: " + moveCounter;
            (int[,], int[,], int[,]) hTables = getHeuristicTableForPossibleMoves(currentState[0], currentState[1], currentState[2]);
            bMoves = getBestMoves(hTables.Item1, hTables.Item2, hTables.Item3);


            listBox1.Items.Clear();
            foreach (Point move in bMoves[0])
            {
                listBox1.Items.Add(move);
            }


            if (bMoves[0].Count > 0) chosenMove = chooseMove(bMoves);
            label2.Text = "Chosen move: " + chosenMove[0];
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == startState.Y[i]) e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == currentState[0].Y[i]) e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }



        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;
            
            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf+1; tar < n; tar++)
                {
                    // get horizontal attackers
                    if (f.Y[rf] == f.Y[tar]) attackers++;
                }
                for (int tar = rf+1; tar < n; tar++)
                {
                    // get diagonal down attackers
                    if (f.Y[tar] == f.Y[rf] + tar - rf) attackers++;
                }
                for (int tar = rf+1; tar < n; tar++)
                {
                    // get diagonal up attackers
                    if (f.Y[rf] == f.Y[tar] + tar - rf) attackers++;
                }
            }
            
            return attackers;
        }

        private (int[,], int[,], int[,]) getHeuristicTableForPossibleMoves(SixState thisState1, SixState thisState2, SixState thisState3)
        {
            int[,] hStates1 = new int[n, n];
            int[,] hStates2 = new int[n, n];
            int[,] hStates3 = new int[n, n];

            for (int i = 0; i < n; i++) // go through the indices
            {
                for (int j = 0; j < n; j++) // replace them with a new value
                {
                    SixState possible = new SixState(thisState1);
                    possible.Y[i] = j;
                    hStates1[i, j] = getAttackingPairs(possible);
                }
            }

            for (int i = 0; i < n; i++) // go through the indices
            {
                for (int j = 0; j < n; j++) // replace them with a new value
                {
                    SixState possible = new SixState(thisState2);
                    possible.Y[i] = j;
                    hStates2[i, j] = getAttackingPairs(possible);
                }
            }

            for (int i = 0; i < n; i++) // go through the indices
            {
                for (int j = 0; j < n; j++) // replace them with a new value
                {
                    SixState possible = new SixState(thisState3);
                    possible.Y[i] = j;
                    hStates3[i, j] = getAttackingPairs(possible);
                }
            }

            return (hStates1, hStates2, hStates3);
        }

        private List<List<Point>> getBestMoves(int[,] heuristicTable1, int[,] heuristicTable2, int[,] heuristicTable3)
        {
            List<(int, Point)> bestMoves1 = new List<(int, Point)>();
            List<(int, Point)> bestMoves2 = new List<(int, Point)>();
            List<(int, Point)> bestMoves3 = new List<(int, Point)>();

            int bestHeuristicValue = heuristicTable1[0, 0];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (bestHeuristicValue > heuristicTable1[i, j])
                    {
                        bestHeuristicValue = heuristicTable1[i, j];
                        bestMoves1.Clear();
                        if (currentState[0].Y[i] != j) bestMoves1.Add((bestHeuristicValue, new Point(i, j)));
                    } else if (bestHeuristicValue == heuristicTable1[i,j])
                    {
                        if (currentState[0].Y[i] != j) bestMoves1.Add((bestHeuristicValue, new Point(i, j)));
                    }
                }
            }

            bestHeuristicValue = heuristicTable2[0, 0];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (bestHeuristicValue > heuristicTable2[i, j])
                    {
                        bestHeuristicValue = heuristicTable2[i, j];
                        bestMoves2.Clear();
                        if (currentState[1].Y[i] != j) bestMoves2.Add((bestHeuristicValue, new Point(i, j)));
                    }
                    else if (bestHeuristicValue == heuristicTable2[i, j])
                    {
                        if (currentState[1].Y[i] != j) bestMoves2.Add((bestHeuristicValue, new Point(i, j)));
                    }
                }
            }

            bestHeuristicValue = heuristicTable3[0, 0];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (bestHeuristicValue > heuristicTable3[i, j])
                    {
                        bestHeuristicValue = heuristicTable3[i, j];
                        bestMoves3.Clear();
                        if (currentState[2].Y[i] != j) bestMoves3.Add((bestHeuristicValue, new Point(i, j)));
                    }
                    else if (bestHeuristicValue == heuristicTable3[i, j])
                    {
                        if (currentState[2].Y[i] != j) bestMoves3.Add((bestHeuristicValue, new Point(i, j)));
                    }
                }
            }
            label5.Text = "Possible Moves (H="+bestHeuristicValue+")";

            List<Point> moves1 = bestMoves1.OrderBy(x => x.Item1).ToList().Select(x => x.Item2).ToList();
            List<Point> moves2 = bestMoves2.OrderBy(x => x.Item1).ToList().Select(x => x.Item2).ToList();
            List<Point> moves3 = bestMoves3.OrderBy(x => x.Item1).ToList().Select(x => x.Item2).ToList();

            List<List<Point>> combinedMoves = new List<List<Point>>();
            combinedMoves.Add(moves1);
            combinedMoves.Add(moves2);
            combinedMoves.Add(moves3);
            return combinedMoves;
        }

        private List<Point> chooseMove(List<List<Point>> possibleMoves)
        {
            List<Point> moves = new List<Point>();
            moves.Add(possibleMoves[0][0]);
            moves.Add(possibleMoves[1][0]);
            moves.Add(possibleMoves[2][0]);
            return moves;
        }

        private void executeMove(List<Point> moves)
        {
            for (int i = 0; i < n; i++) startState.Y[i] = currentState[0].Y[i];
            currentState[0].Y[moves[0].X] = moves[0].Y;

            //for (int i = 0; i < n; i++) startState.Y[i] = currentState[1].Y[i];
            currentState[1].Y[moves[1].X] = moves[1].Y;

            //for (int i = 0; i < n; i++) startState.Y[i] = currentState[2].Y[i];
            currentState[2].Y[moves[2].X] = moves[2].Y;

            moveCounter++;

            //chosenMove = null;
            updateUI();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (getAttackingPairs(currentState[0]) > 0) executeMove(chosenMove);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            currentState.Clear();
            for (int i = 0; i < k; i++) currentState.Add(randomSixState());
            startState = currentState[0];

            moveCounter = 0;

            updateUI();
            pictureBox1.Refresh();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int x1 = getAttackingPairs(currentState[0]);
            int x2 = getAttackingPairs(currentState[1]);
            int x3 = getAttackingPairs(currentState[2]);
            while (true)
            {
                if (x1 <= 0 || x2 <= 0 || x3 <= 0) break;

                executeMove(chosenMove);
                x1 = getAttackingPairs(currentState[0]);
                x2 = getAttackingPairs(currentState[1]);
                x3 = getAttackingPairs(currentState[2]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
