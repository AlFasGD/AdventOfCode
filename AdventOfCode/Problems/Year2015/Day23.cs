﻿namespace AdventOfCode.Problems.Year2015
{
    public class Day23 : Problem<int>
    {
        private GenericComputer computer = new();
        private ComputerInstruction[] instructions;

        public override int SolvePart1()
        {
            computer.LoadRunProgram(instructions);
            return computer.GetRegisterValue('b');
        }
        public override int SolvePart2()
        {
            computer.ResetRegisters();
            computer.SetRegisterValue('a', 1);
            computer.RunProgram();
            return computer.GetRegisterValue('b');
        }

        protected override void LoadState()
        {
            instructions = ParsedFileLines(s => ComputerInstruction.Parse(s, ", "));
        }
        protected override void ResetState()
        {
            instructions = null;
        }
    }
}
