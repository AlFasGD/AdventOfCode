﻿using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day14 : Problem2<ulong>
    {
        private MemorySystem memorySystem;

        public override ulong SolvePart1()
        {
            memorySystem = new MemorySystem(FileLines.Select(MemorySystemCommand.ParseBase));
            return RunCommandsForSystem();
        }
        public override ulong SolvePart2()
        {
            memorySystem = new MemorySystemVersion2(FileLines.Select(MemorySystemCommand.ParseBase));
            return RunCommandsForSystem();
        }

        private ulong RunCommandsForSystem()
        {
            memorySystem.RunAllCommands();
            return memorySystem.MemoryValuesSum;
        }

        protected override void ResetState()
        {
            memorySystem = null;
        }

        #region Memory Systems
        private class MemorySystemVersion2 : MemorySystem
        {
            public MemorySystemVersion2(IEnumerable<MemorySystemCommand> commands)
                : base(commands) { }

            public override void RunCommand(MemorySystemCommand command)
            {
                if (command is not MemoryWriteCommand writeCommand)
                {
                    base.RunCommand(command);
                    return;
                }

                var address = writeCommand.GetVersion2MemoryAddress(CurrentMask);
                var combinations = BitManipulations.GetCombinationsFromMask(address.FloatingBitsMask, 36);
                foreach (var c in combinations)
                    Memory[address.MaskedBaseMemoryAddress | c] = writeCommand.NewValue;
            }
        }
        private class MemorySystem
        {
            private readonly MemorySystemCommand[] memoryCommands;
            protected readonly FlexibleDictionary<ulong, ulong> Memory = new();

            protected Bitmask CurrentMask = new();

            // In the following functions

            // Sum<TSource, TSummable>(this IEnumerable<TSource> source, Func<TSource, TSummable> selector);
            // Sum<TSource, TSummable>(this IEnumerable<TSource> source, Func<TSource, TSummable?> selector)
            //     where TSummable : struct;

            // LINQ does not provide overloads when
            // TSummable: byte, sbyte, short, ushort, uint, ulong

            // What went wrong, Microsoft?
            public ulong MemoryValuesSum => Memory.Sum(kvp => kvp.Value);

            public MemorySystem(IEnumerable<MemorySystemCommand> commands)
            {
                memoryCommands = commands.ToArray();
            }

            public void RunAllCommands()
            {
                foreach (var command in memoryCommands)
                    RunCommand(command);
            }
            public virtual void RunCommand(MemorySystemCommand command)
            {
                switch (command)
                {
                    case MemoryWriteCommand writeCommand:
                        Memory[writeCommand.MemoryAddress] = writeCommand.GetMaskedNewValue(CurrentMask);
                        break;
                    case MaskSetCommand maskSetCommand:
                        CurrentMask = maskSetCommand.NewMask;
                        break;
                }
            }

            public static MemorySystem Parse(string[] commands)
            {
                return new(commands.Select(MemorySystemCommand.ParseBase));
            }
        }
        #endregion

        #region Commands
        private abstract class MemorySystemCommand
        {
            public static MemorySystemCommand ParseBase(string command)
            {
                if (command.StartsWith("mask"))
                    return MaskSetCommand.Parse(command);
                if (command.StartsWith("mem"))
                    return MemoryWriteCommand.Parse(command);
                return null;
            }
        }
        private class MemoryWriteCommand : MemorySystemCommand
        {
            public ulong MemoryAddress { get; }
            public ulong NewValue { get; }

            public MemoryWriteCommand(ulong memoryAddress, ulong newValue)
            {
                MemoryAddress = memoryAddress;
                NewValue = newValue;
            }

            public ulong GetMaskedNewValue(Bitmask mask) => mask.ApplyToValue(NewValue);
            public MaskedMemoryAddress GetVersion2MemoryAddress(Bitmask mask) => new(MemoryAddress | mask.RawValueMask, mask.XMask);

            public static MemoryWriteCommand Parse(string command)
            {
                var split = command.Split(" = ");
                int indexerStart = split[0].IndexOf('[') + 1;
                ulong memoryAddress = split[0][indexerStart..^1].ParseUInt32();
                ulong value = split[1].ParseUInt64();
                return new(memoryAddress, value);
            }

            public override string ToString() => $"mem[{MemoryAddress}] = {NewValue}";
        }
        private class MaskSetCommand : MemorySystemCommand
        {
            public Bitmask NewMask { get; }

            public MaskSetCommand(Bitmask newMask)
            {
                NewMask = newMask;
            }

            public static MaskSetCommand Parse(string command)
            {
                return new(Bitmask.Parse(command.Split(" = ").Last()));
            }

            public override string ToString() => $"mask = {NewMask}";
        }
        #endregion

        #region Records but not really records
        private class MaskedMemoryAddress
        {
            public ulong RawMemoryAddress { get; }
            public ulong FloatingBitsMask { get; }
            public ulong MaskedBaseMemoryAddress => RawMemoryAddress & (~FloatingBitsMask);

            public MaskedMemoryAddress(ulong rawMemoryAddress, ulong floatingBitsMask)
            {
                RawMemoryAddress = rawMemoryAddress;
                FloatingBitsMask = floatingBitsMask;
            }

            public override string ToString()
            {
                const int BitLength = 36;

                char[] chars = new char[BitLength];
                ulong currentMask = 1;
                for (int i = 0; i < BitLength; i++, currentMask <<= 1)
                {
                    chars[BitLength - 1 - i] = (FloatingBitsMask & currentMask, RawMemoryAddress & currentMask) switch
                    {
                        (> 0, _) => 'X',
                        (_, > 0) => '1',
                        (_, _) => '0',
                    };
                }
                return new(chars);
            }
        }

        private class Bitmask
        {
            public ulong XMask { get; }
            public ulong RawValueMask { get; }

            public Bitmask() 
                : this(default, default) { }

            public Bitmask(ulong xMask, ulong rawValueMask)
            {
                XMask = xMask;
                RawValueMask = rawValueMask;
            }

            public ulong ApplyToValue(ulong value)
            {
                return (value & XMask) | RawValueMask;
            }
            public ulong ApplyToMemoryAddress(ulong memoryAddress)
            {
                return memoryAddress | RawValueMask;
            }

            // This better be useless
            public override string ToString()
            {
                const int BitLength = 36;

                char[] chars = new char[BitLength];
                ulong currentMask = 1;
                for (int i = 0; i < BitLength; i++, currentMask <<= 1)
                {
                    chars[BitLength - 1 - i] = (XMask & currentMask, RawValueMask & currentMask) switch
                    {
                        (> 0, _) => 'X',
                        (_, > 0) => '1',
                        (_, _) => '0',
                    };
                }
                return new(chars);
            }

            public static Bitmask Parse(string rawMask)
            {
                ulong xMask = 0;
                ulong rawValueMask = 0;

                ulong currentMaskBit = 1;
                for (int i = rawMask.Length - 1; i >= 0; i--, currentMaskBit <<= 1)
                {
                    switch (rawMask[i])
                    {
                        case 'X':
                            xMask |= currentMaskBit;
                            break;
                        case '1':
                            rawValueMask |= currentMaskBit;
                            break;
                    }
                }

                return new(xMask, rawValueMask);
            }
        }
        #endregion
    }
}