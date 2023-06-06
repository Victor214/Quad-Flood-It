using Game;
using Solver;

namespace Testing
{
    public class StateTesting
    {
        [Fact]
        public void StateTesting_AreNotEqual()
        {
            string boardText = File.ReadAllText(@"Boards/example1_6_6_3.txt");
            Board board = new Board(boardText);

            State state1 = new State(board);
            state1.Actions.Add(new Solver.Action("a", 1));

            State state2 = new State(board);
            state1.Actions.Add(new Solver.Action("a", 2));

            Assert.NotEqual(state1, state2);
        }

        [Fact]
        public void StateTesting_AreEqual()
        {
            string boardText = File.ReadAllText(@"Boards/example1_6_6_3.txt");
            Board board = new Board(boardText);

            //State state1 = new State(board);
            //state1.Actions.Add(new Solver.Action("a", 1));

            //State state2 = new State(board);
            //state1.Actions.Add(new Solver.Action("a", 1));

            //Assert.Equal(state1, state2);


            State state3 = new State(board);
            state3.Actions.Add(new Solver.Action("a", 3));
            state3.Actions.Add(new Solver.Action("a", 2));
            state3.Actions.Add(new Solver.Action("a", 3));
            state3.Actions.Add(new Solver.Action("a", 1));
            state3.ClearBoard();

            var x = state3.CurrentBoard;

            //State state4 = new State(board);
            //state4.Actions.Add(new Solver.Action("a", 3));
            //state4.Actions.Add(new Solver.Action("a", 2));
            //state4.Actions.Add(new Solver.Action("a", 3));
            //state4.Actions.Add(new Solver.Action("a", 1));
            //state4.Actions.Add(new Solver.Action("a", 3));
            //state4.Actions.Add(new Solver.Action("a", 1));

            //Assert.Equal(state3, state4);
        }
    }
}