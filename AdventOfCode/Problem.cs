﻿using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using Garyon.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode
{
    public abstract class Problem
    {
        private const string runPartMethodPrefix = "RunPart";

        private int currentTestCase;

        protected bool StateLoaded { get; private set; }

        protected int CurrentTestCase
        {
            get => currentTestCase;
            set
            {
                if (currentTestCase == value)
                    return;

                currentTestCase = value;
                ResetLoadedState();
            }
        }

        protected string BaseDirectory => $@"Inputs\{Year}";
        protected string FileContents => GetFileContents(CurrentTestCase);
        protected string NormalizedFileContents => GetFileContents(CurrentTestCase).NormalizeLineEndings();
        protected string[] FileLines => GetFileLines(CurrentTestCase);
        protected int[] FileNumbersInt32 => ParsedFileLines(int.Parse);
        protected long[] FileNumbersInt64 => ParsedFileLines(long.Parse);

        public int Year => GetType().Namespace.Split('.').Last()[^4..].ParseInt32();
        public int Day => GetType().Name["Day".Length..].ParseInt32();
        public int TestCaseFiles => Directory.GetFiles(BaseDirectory).Where(f => f.Replace('\\', '/').Split('/').Last().StartsWith($"{Day}T")).Count();

        protected T[] ParsedFileLines<T>(Parser<T> parser) => ParsedFileLinesEnumerable(parser).ToArray();
        protected T[] ParsedFileLines<T>(Parser<T> parser, int skipFirst, int skipLast) => ParsedFileLinesEnumerable(parser, skipFirst, skipLast).ToArray();
        protected IEnumerable<T> ParsedFileLinesEnumerable<T>(Parser<T> parser) => ParsedFileLinesEnumerable(parser, 0, 0);
        protected IEnumerable<T> ParsedFileLinesEnumerable<T>(Parser<T> parser, int skipFirst, int skipLast) => FileLines.Skip(skipFirst).SkipLast(skipLast).Select(new Func<string, T>(parser));

        public object[] SolveAllParts(bool displayExecutionTimes = true) => SolveAllParts(0, displayExecutionTimes);
        public object[] SolveAllParts(int testCase, bool displayExecutionTimes = true)
        {
            var methods = GetType().GetMethods().Where(m => m.Name.StartsWith(runPartMethodPrefix)).ToArray();
            var result = new object[methods.Length];

            CurrentTestCase = testCase;
            DisplayExecutionTimes(displayExecutionTimes, "State loading", EnsureLoadedState);

            for (int i = 0; i < result.Length; i++)
            {
                DisplayExecutionTimes(displayExecutionTimes, $"Part {i + 1}", () =>
                {
                    result[i] = methods[i].Invoke(this, null);
                });
            }
            return result;
        }

        private static void DisplayExecutionTimes(bool displayExecutionTimes, string title, Action action)
        {
            if (!displayExecutionTimes)
                return;

            var executionTime = BasicBenchmarking.MeasureExecutionTime(action);
            Console.WriteLine($"{title} execution time: {executionTime.TotalMilliseconds:N2} ms");
        }

        protected virtual void LoadState() { }
        protected virtual void ResetState() { }

        protected void EnsureLoadedState()
        {
            HandleStateLoading(true, LoadState);
        }
        private void ResetLoadedState()
        {
            HandleStateLoading(false, ResetState);
        }

        private void HandleStateLoading(bool targetStateLoadedStatus, Action stateHandler)
        {
            if (StateLoaded == targetStateLoadedStatus)
                return;
            stateHandler();
            StateLoaded = targetStateLoadedStatus;
        }

        private string GetFileContents(int testCase) => File.ReadAllText(GetFileLocation(testCase));
        private string[] GetFileLines(int testCase) => GetFileContents(testCase).GetLines();

        private string GetFileLocation(int testCase) => $"{BaseDirectory}/{Day}{(testCase > 0 ? $"T{testCase}" : "")}.txt";
    }

    public abstract class Problem<T1, T2> : Problem
    {
        public T1 RunPart1()
        {
            EnsureLoadedState();
            return SolvePart1();
        }
        public T2 RunPart2()
        {
            EnsureLoadedState();
            return SolvePart2();
        }

        public abstract T1 SolvePart1();
        public abstract T2 SolvePart2();
    }

    public abstract class Problem<T> : Problem<T, T> { }
}
