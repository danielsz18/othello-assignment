#nullable enable
using System;
using static System.Console;

namespace Bme121
{
    static partial class pa1
    {
        
        static void Main( )
        {
            string[,]? game = null;
            int rows, cols;
            string playerW = "";
            string playerB = "";

            Console.Clear();
            WriteLine("Welcome, please enter the information below to set up Othello.");
           
            while (playerW == ""){ //If player inputs no name, they will be re-prompted
                Write("Enter player name (white): ");
                playerW = ReadLine() !;
            }
            
            while (playerB == ""){
                Write("Enter player name (black): ");
                playerB = ReadLine() !;
            }
    
            //Re-collect board size until valid numbers are entered
            while (game == null){
                Write("Enter the amount of rows in the board: ");
                rows = int.Parse( ReadLine() ) !;
                Write("Enter the amount of columns in the board: ");
                cols = int.Parse( ReadLine() ) !;
            
                game = NewBoard( rows, cols ); //Will return null array if the board is invalid

                if(game == null){
                    WriteLine();
                    WriteLine("The number of rows/columns must each be even numbers between 4 and 26.");
                }
            }
            
            //Display Initial Board
            Console.Clear( );
            WriteLine( );
            WriteLine( " Welcome to Othello!" );
            WriteLine( );
            DisplayBoard( game );
            WriteLine( );
            
            bool gameOver = false;
            bool whiteTurn = true;
            string moveInput = "";
            
            //Game loop
            while (!gameOver)
            {
                
                
                DisplayScores( game, playerW, playerB );
                WriteLine();                
                
                if(whiteTurn) Write($" {playerW}'s turn! Place an O token ");
                else Write($" {playerB}'s turn! Place an X token ");
                WriteLine("by inputting a move below. (Format is two letters: row and then column, i.e. ab)");
                
                bool turnOver = false;

                while(!turnOver){
                    moveInput = ReadLine() !;
                
                    if ( moveInput == "quit"){
                        gameOver = true; //End game
                        turnOver = true;
                    }    
                    else if ( moveInput == "skip") {
                        turnOver = true;
                    }
                    //Checks for validity of move
                    else if (ValidMove(game, moveInput, whiteTurn)) {
                        //Completes player move
                        game = PlayerMove(game, moveInput, whiteTurn);
                        turnOver = true;
                    }
                    else    WriteLine("  Invalid move! Please try again."); 
                }
                
                if(!gameOver)
                {
                    DisplayBoard( game );
                    whiteTurn = !whiteTurn; //Toggles player turn
                
                    //Check to see if the next player's turn has no valid moves
                    if( NoValidMoves (game, whiteTurn) ){
                        
                        if (whiteTurn) WriteLine($"  No valid moves for {playerW}! Skipping turn...");
                        else WriteLine($"  No valid moves for {playerB}! Skipping turn...");
                        
                        whiteTurn = !whiteTurn; 
                        
                        //Check again to see if the next player has no valid moves, which would end the game
                        if( NoValidMoves ( game, whiteTurn)){
                            WriteLine("  Neither player can move! Ending game...");
                            gameOver = true;
                        }
                    }
                }
            } 
            
            GameOver(game, playerW, playerB);
        }        
        
        static string[,] PlayerMove(string[,] game, string moveInput, bool whiteTurn)
        {
            //Array stores selected row and column.
            int[] selected = new int[2] { IndexAtLetter( moveInput.Substring(0, 1) ) , IndexAtLetter( moveInput.Substring(1) ) };
           
            if(whiteTurn)
            {
                //Add selected tile
                game[selected[0], selected[1]] = "O";
                
                //Tests all 8 directions for surrounding enemy pieces
                //Ranges from -1 to 1 in each direction
                for( int[] d = new int [2] {-1, -1} ; d[0] < 2 ; d[0]++ ){
                    
                    while( d[1] < 2 ){
                        if(FlipTest(game, selected, d, whiteTurn)){

                            int[] flipper = new int[2] { selected[0] + d[0], selected[1] + d[1] } ;
                            
                            while(game[ flipper[0], flipper[1] ] == "X"){
                                game[ flipper[0], flipper[1] ] = "O";

                                flipper[0] += d[0];
                                flipper [1] += d[1];
                            }
                       
                        }
                       
                        d[1] ++;
                    }
                    
                    d[1] = -1; //Reset column direction for the while loop to replay
                }
            }
            else
            {
                game[selected[0], selected[1]] = "X";
                
                //Tests all 8 directions for surrounding enemy pieces
                //Ranges from -1 to 1 in each direction
                
                for( int[] d = new int [2] {-1, -1} ; d[0] < 2 ; d[0]++){
                    
                    while( d[1] < 2 ){
                        if( FlipTest(game, selected, d, whiteTurn) ){
 
                            int[] flipper = new int[2]{selected[0] + d[0], selected[1] + d[1]};
                            
                            while(game[ flipper[0], flipper[1] ] == "O"){
                                game[ flipper[0], flipper[1] ] = "X";

                                flipper[0] += d[0];
                                flipper [1] += d[1];
                            }
                       
                        }
                       
                        d[1] ++;
                    }
                    
                    d[1] = -1; //Reset column for the while loop to replay
                }
            }
            
            return game;
        }
        
        static bool ValidMove(string[,] game, string moveInput, bool whiteTurn)
        {
           //Only accepts two-character answers.
           if (moveInput.Length != 2) return false;
           
           //Store selected tile in an array
           int[] selected = new int[2] { IndexAtLetter( moveInput.Substring(0, 1) ) , IndexAtLetter( moveInput.Substring(1) ) }; 
           
           if(selected[0] >= game.GetLength(0) || selected[0] < 0 || selected[1] >= game.GetLength(1) || selected[1] < 0 //Must be in the array bounds
           || game[selected[0], selected[1]] == "O" || game[selected[0], selected[1]] == "X" ) return false; //Can only place tiles on empty squares
           
           for( int[] d = new int [2] {-1, -1} ; d[0] < 2 ; d[0]++){
                while( d[1] < 2 ){
                        //If any direction is flippable, returns that the move is valid
                        if( FlipTest(game, selected, d, whiteTurn) ) return true;
                        d[1] ++;
                    }
                d[1] = -1;
            }
            return false;
           
       }
        
        //Checks whether the selected tile has surrounding enemy pieces that can be flipped
        static bool FlipTest(string[,] game, int[] selected, int[] direction, bool whiteTurn)
        {
            int[] test = new int[2] { selected[0], selected[1] }; //Copy selected into new 'test' array
            
            test[0] += direction[0];
            test[1] += direction[1];
            
            //Check if the test value is out of bounds so that no errors will occur when checking game values
            if(test[0] >= game.GetLength(0) || test[0] < 0 || test[1] >= game.GetLength(1) || test[1] < 0) return false;
            
            if( game[test[0], test[1]] == "O"){ 
                //If it's white's turn, O is their tile and is ignored
                if ( whiteTurn ) return false; 
                
                while( game[test[0], test[1]] == "O"){
                    test[0] += direction[0];
                    test[1] += direction[1];
                    
                    //If the test value goes out of bounds, no enemy token was detected so no flip can occur
                    if(test[0] >= game.GetLength(0) || test[0] < 0 || test[1] >= game.GetLength(1) || test[1] < 0) return false;
                }
                
                if( game[test[0], test[1]] == "X") return true; //Player token surrounding enemy tokens, therefore the flip is valid
                else return false; //End of the chain is blank.
                
            }    
            else if( game[test[0], test[1]] == "X"){
                //If it's black's turn, X is their tile and is ignored
                if ( !whiteTurn ) return false; 
                
                while(game[test[0], test[1]] == "X"){
                    test[0] += direction[0];
                    test[1] += direction[1];
                    
                    if(test[0] == game.GetLength(0) || test[0] < 0 || test[1] == game.GetLength(1) || test[1] < 0) return false;
                }
   
                if(game[test[0], test[1]] == "O") return true;
                else return false; 
            
            }
            else return false; //Adjacent square is empty 
       }
        
        //Displays the scores of each player
        static void DisplayScores( string[,] game, string playerW, string playerB )
        {
            int wScore = 0;
            int bScore = 0;
            
            foreach( string tile in game ){
                if (tile == "O")  wScore += 1;
                if (tile == "X")  bScore += 1;
            }
            
            WriteLine($" Scores --  {playerW}: {wScore}   {playerB}: {bScore}");
        }
    
        //Check to see if any valid moves remain for a player.
        static bool NoValidMoves(string[,] game, bool whiteTurn)
        {  
            for(int r = 0; r < game.GetLength(0); r++){
                for(int c = 0; c < game.GetLength(1); c++){
                
                    if( game[r, c] != "O" && game[r, c] != "X"){  //Must be an empty tile
                        //Create a 'moveInput' string for testing and then check if the move is valid
                        string selected = $"{ LetterAtIndex(r) }{ LetterAtIndex(c) }"; 
                        if ( ValidMove(game, selected, whiteTurn) ) return false; 
                    }
                }   
            }
            //Returns true if ValidMove never returns false in the loop.
            return true;
        }
       
        //Display final scores and winner
        static void GameOver(string[,] game, string playerW, string playerB)
        {   
            int wScore = 0;
            int bScore = 0;

            WriteLine();
            WriteLine();
            WriteLine(" FINISHED!");
            WriteLine();
            
            foreach( string tile in game ){
                if (tile == "O")  wScore += 1;
                if (tile == "X")  bScore += 1;
            }
            
            WriteLine($"  Final Scores --  {playerW}: {wScore}   {playerB}: {bScore}");
            WriteLine();
            
            if (wScore > bScore) WriteLine($"   {playerW} wins by {wScore - bScore} tokens!");

            else if (wScore < bScore) WriteLine($"   {playerB} wins by {bScore - wScore} tokens!");
            
            else WriteLine($"   Draw!"); //If wScore == bScore, game ends in a draw

            WriteLine();
            
        }
        
        // -----------------------------------------------------------------------------------------
        // Return the single-character string "a".."z" corresponding to its index 0..25. 
        // Return " " for an invalid index.
        
        static string LetterAtIndex( int number )
        {
            if( number < 0 || number > 25 ) return " ";
            else return "abcdefghijklmnopqrstuvwxyz"[ number ].ToString( );
        }
        
        // -----------------------------------------------------------------------------------------
        // Return the index 0..25 corresponding to its single-character string "a".."z". 
        // Return -1 for an invalid string.
        
        static int IndexAtLetter( string letter ) 
        {
            return "abcdefghijklmnopqrstuvwxyz".IndexOf(letter);
        }
        
        // -----------------------------------------------------------------------------------------
        // Create a new Othello game board, initialized with four pieces in their starting
        // positions. The counts of rows and columns must be no less than 4, no greater than 26,
        // and not an odd number. If not, the new game board is created as an empty array.
        
        static string[ , ] NewBoard( int rows, int cols )
        {
            const string blank = " ";
            const string white = "O";
            const string black = "X";
            
            if(    rows < 4 || rows > 26 || rows % 2 == 1
                || cols < 4 || cols > 26 || cols % 2 == 1 ) return null!;
                
            string[ , ] board = new string[ rows, cols ];
            
            for( int row = 0; row < rows; row ++ )
            {
                for( int col = 0; col < cols; col ++ )
                {
                    board[ row, col ] = blank;
                }
            }
            
            board[ rows / 2 - 1, cols / 2 - 1 ] = white;
            board[ rows / 2 - 1, cols / 2     ] = black;
            board[ rows / 2,     cols / 2 - 1 ] = black;
            board[ rows / 2,     cols / 2     ] = white;
            
            return board;
        }

        // -----------------------------------------------------------------------------------------
        // Display the Othello game board on the Console.
        // All information about the game is held in the two-dimensional string array.
        
        static void DisplayBoard( string[ , ] board )
        {
            const string h  = "\u2500"; // horizontal line
            const string v  = "\u2502"; // vertical line
            const string tl = "\u250c"; // top left corner
            const string tr = "\u2510"; // top right corner
            const string bl = "\u2514"; // bottom left corner
            const string br = "\u2518"; // bottom right corner
            const string vr = "\u251c"; // vertical join from right
            const string vl = "\u2524"; // vertical join from left
            const string hb = "\u252c"; // horizontal join from below
            const string ha = "\u2534"; // horizontal join from above
            const string hv = "\u253c"; // horizontal vertical cross
            const string mx = "\u256c"; // marked horizontal vertical cross
            const string sp =      " "; // space

            // Nothing to display?
            if( board == null ) return;
            
            int rows = board.GetLength( 0 );
            int cols = board.GetLength( 1 );
            if( rows == 0 || cols == 0 ) return;
            
            // Display the board row by row.
            for( int row = 0; row < rows; row ++ )
            {
                if( row == 0 )
                {
                    // Labels above top edge.
                    for( int col = 0; col < cols; col ++ )
                    {
                        if( col == 0 ) Write( "   {0}{0}{1}{0}", sp, LetterAtIndex( col ) );
                        else Write( "{0}{0}{1}{0}", sp, LetterAtIndex( col ) );
                    }
                    WriteLine( );
                    
                    // Border above top row.
                    for( int col = 0; col < cols; col ++ )
                    {
                        if( col == 0 ) Write( "   {0}{1}{1}{1}", tl, h );
                        else Write( "{0}{1}{1}{1}", hb, h );
                        if( col == cols - 1 ) Write( "{0}", tr );
                    }
                    WriteLine( );
                }
                else
                {
                    // Border above a row which is not the top row.
                    for( int col = 0; col < cols; col ++ )
                    {
                        if(    rows > 5 && cols > 5 && row ==        2 && col ==        2 
                            || rows > 5 && cols > 5 && row ==        2 && col == cols - 2 
                            || rows > 5 && cols > 5 && row == rows - 2 && col ==        2 
                            || rows > 5 && cols > 5 && row == rows - 2 && col == cols - 2 )  
                            Write( "{0}{1}{1}{1}", mx, h );
                        else if( col == 0 ) Write( "   {0}{1}{1}{1}", vr, h );
                        else Write( "{0}{1}{1}{1}", hv, h );
                        if( col == cols - 1 ) Write( "{0}", vl );
                    }
                    WriteLine( );
                }
                
                // Row content displayed column by column.
                for( int col = 0; col < cols; col ++ ) 
                {
                    if( col == 0 ) Write( " {0,-2}", LetterAtIndex( row ) ); // Labels on left side
                    Write( "{0} {1} ", v, board[ row, col ] );
                    if( col == cols - 1 ) Write( "{0}", v );
                }
                WriteLine( );
                
                if( row == rows - 1 )
                {
                    // Border below last row.
                    for( int col = 0; col < cols; col ++ )
                    {
                        if( col == 0 ) Write( "   {0}{1}{1}{1}", bl, h );
                        else Write( "{0}{1}{1}{1}", ha, h );
                        if( col == cols - 1 ) Write( "{0}", br );
                    }
                    WriteLine( );
                }
            }
        }
    }
}
