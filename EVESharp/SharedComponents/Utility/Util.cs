﻿/*
 * ---------------------------------------
 * User: duketwo
 * Date: 23.03.2014
 * Time: 14:20
 *
 * ---------------------------------------
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OpenQA.Selenium;
using SharedComponents.Extensions;
using SharedComponents.IPC;
using SharedComponents.WinApiUtil;

namespace SharedComponents.Utility
{
    /// <summary>
    ///     Description of Util.
    /// </summary>
    public class Util
    {
        [DllImport("KERNEL32.DLL", EntryPoint =
         "SetProcessWorkingSetSize", SetLastError = true,
         CallingConvention = CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize32Bit
         (IntPtr pProcess, int dwMinimumWorkingSetSize,
         int dwMaximumWorkingSetSize);

        [DllImport("KERNEL32.DLL", EntryPoint =
           "SetProcessWorkingSetSize", SetLastError = true,
           CallingConvention = CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize64Bit
           (IntPtr pProcess, long dwMinimumWorkingSetSize,
           long dwMaximumWorkingSetSize);

        public static void FlushMem()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize64Bit(Process.GetCurrentProcess().Handle, -1, -1);
        }

        public static void FlushMemIfThisProcessIsUsingTooMuchMemory(int tooMuchMemoryThresholdInMB)
        {
            var ramAllocation = Process.GetCurrentProcess().WorkingSet64;
            var allocationInMB = ramAllocation / (1024 * 1024);

            if (allocationInMB > tooMuchMemoryThresholdInMB)
            {
                FlushMem();
            }
        }

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        public const int SW_RESTORE = 9;
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 1;
        public const int SW_SHOWNOACTIVATE = 4;
        public static Random _random = new Random();


        /// <summary>
        ///     Check to run certain code at a given interval
        ///     Limitation: Due the fact the line number is used as part of the reference, this
        ///     method can only be used once per line.
        /// </summary>
        /// <param name="delayMs">The requested interval.</param>
        /// <param name="delayMsMax">If set the interval will be randomized between (delayMs, delayMsMax). Max val is exclusive!</param>
        /// <param name="uniqueName">Instead of the combination of CallerMemberName and CallerLineNumber a unique string can be used. That
        /// way the Interval can be used at multiple locations.</param>
        /// <param name="ln">Internal use only</param>
        /// <param name="caller">Internal use only</param>
        /// <returns></returns>
        public static bool IntervalInMilliseconds(int delayMs, int delayMsMax = 0, string uniqueName = null, [CallerLineNumber] int ln = 0, [CallerMemberName] string caller = null, [CallerFilePath] string callerFilePath = null)
        {
            //caller = uniqueName ?? caller + ln.ToString();
            caller = uniqueName ?? caller + ln.ToString() + callerFilePath;

            var now = DateTime.UtcNow;
            var delay = delayMsMax == 0 ? delayMs : _random.Next(delayMs, delayMsMax);

            if (_intervalDict.TryGetValue(caller, out var dt) && dt > now)
                return false;

            _intervalDict[caller] = now.AddMilliseconds(delay);
            return true;
        }

        public static bool IntervalInMinutes(int delayMinutes, int delayMinutesMax = 0, string uniqueName = null, [CallerLineNumber] int ln = 0, [CallerMemberName] string caller = null, [CallerFilePath] string callerFilePath = null)
        {
            //caller = uniqueName ?? caller + ln.ToString();
            caller = uniqueName ?? caller + ln.ToString() + callerFilePath;

            var now = DateTime.UtcNow;
            var delay = delayMinutesMax == 0 ? delayMinutes : _random.Next(delayMinutes, delayMinutesMax);

            if (_intervalDict.TryGetValue(caller, out var dt) && dt > now)
                return false;

            _intervalDict[caller] = now.AddMinutes(delay);
            return true;
        }

        private static Dictionary<string, DateTime> _intervalDict = new Dictionary<string, DateTime>();

        public static ulong ElapsedMicroSeconds(Stopwatch watch)
        {
            return (ulong)Math.Round(((double)watch.ElapsedTicks / Stopwatch.Frequency) * 1000000, 0);
        }

        private static string _assemblyPath;

        public Util()
        {
        }

        public static void GlobalRemoteLog(string s)
        {
           WCFClient.Instance.GetPipeProxy.RemoteLog(s);
        }

        /// <summary>
        /// 50/50 flip
        /// </summary>
        /// <returns></returns>
        public static bool Coinflip()
        {
            return _random.NextDouble() > 0.5d;
        }

        public static void PlayNoticeSound()
        {
            PlaySound("UklGRpCZAABXQVZFZm10IBAAAAABAAIAgD4AAAB9AAACAAgATElTVBoAAABJTkZPSVNGVA4AAABMYXZmNTguNzYuMTAwAGRhdGFKmQAAf3+AgICAgICAgIB/f3+AgICAgICAgICAgICAgIB/f4B/gH9/f3+Af4CAgICAgICAgICAgICAgH+AgIB/gIB/gH9/f39/f39/gH9/f39/gICAgICAgICAgICAgIB/gH9/f39/f39/f39/f39/f39/f39/f3+AgH9/f39/f39/f39/f39/f39/f4B/f39/f39/f3+AgICAf4CAgH9/f39/f39/f39/f39/gH9/f39/f39/f39/f39/f39/f4B/gICAf4B/f39/f39/f39/f39/f39/f39/f4CAgIB/f39/f39/f39/f39/f3+AgICAf4B/gH+AgICAgIB/gH9/f39/f39/f39/gH9/f39/f4B/gICAgH9/f4CAgICAf39/f39/f39/f39/f39/f39/f39/f39/gH+AgICAgICAgICAgICAgH+Af4CAgICAgH9/f39/gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgH+Af4CAgIB/gH+AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgH9/f39/f4CAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIB/gICAgICAgICAgICAgICAgICAf39/f39/f3+AgICAgICAgICAgICAgICAgH+AgIB/gH+Af4B/gH+AgICAgICAgICAgICAgICAgICAgICAgICAgH9/f39/f39/gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAf4B/gICAgICAgICAgICAgICAf39/gICAgICAgICAgICAgIB/f39/f39/f39/f39/f39/f39/f4CAgICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f4CAgICAgICAgIB/f39/f39/f4CAgICAgICAgIB/f39/f39/f4CAgICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f4CAgICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f4B/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/gH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f4B/gICAgICAf4B/gH9/f3+Af4B/gICAgICAf4B/gH9/f3+Af4B/gICAgICAf4B/gH9/f39/f4B/gICAgICAf4B/gH9/f39/f4B/gH+AgICAgICAgICAgICAf39/f39/f4CAgICAgICAgIB/f39/f39/f4CAgICAgICAgIB/f39/f39/f3+AgICAgICAgIB/f39/f39/f3+AgICAgICAgIB/f39/f39/f39/gICAgICAgICAgH9/f39/f39/f4CAgICAgICAgH9/f39/f39/gICAgICAgIB/gH9/f39/f39/gICAgICAgIB/f39/f39/f39/gICAgICAgIB/gH9/f39/f39/gICAgICAgIB/gH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/f4CAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/gICAgICAgICAgH9/f39/f39/f4CAgICAgICAgH9/f39/f39/f4CAgICAgICAgH9/f39/f39/f3+AgICAgICAgH9/f39/f39/f4CAgICAgICAgH9/f39/f39/f3+AgICAgICAgH9/f39/f39/f3+AgICAgICAgH9/f39/f39/f3+AgICAgICAgH9/f39/f39/f3+AgICAgICAgH9/f39/f39/f3+AgICAgICAgH9/f39/f39/f3+AgICAgICAgH9/f39/f39/f3+AgICAgICAgIB/f39/f39/f3+AgICAgICAgICAf39/f39/f3+AgICAgICAgICAf39/f39/f39/gICAgICAgH+Af39/f39/f3+AgICAgICAgICAf39/f39/f3+AgICAgICAgICAf39/f39/f3+AgICAgICAgICAf39/f39/f39/f4CAgICAgICAgH9/f39/f39/f4CAgICAgICAgH9/f39/f39/f4CAgICAgICAf39/f39/f39/f4CAgICAgICAgH9/f39/f39/f4CAgICAgICAgIB/f39/f39/f39/gICAgICAgIB/f39/f39/f3+AgICAgICAgIB/f39/f39/f3+AgICAgICAgIB/f39/f39/f39/gICAgIGAgYGAgH+Af39+fn5+f39/f4CAgICAgICAgICAgICAgIB/f39/f39/f39/f39/f4CAgICBgYGAgIB/f35+fn5/f39/gICAgICAgICAgICAf39/f39/f39/f4B/gICAgH+AgICAgICAgICAf39/f39/f39/f39/f4CAgICBgYGBgIB/f39/fn5+fn9/gH+AgICAgICAgICAf4B/f39/f39/f4B/gH+AgH+Af4CAgICAgICAgICAf39/f35/fn9/f39/gICBgIGBgYGAgH9/fn9+fn9/f3+AgICAgICAgICAgIB/gH9/f39/f4B/gH+Af3+Af4B/gICAgICBgICAgH9/f35/fn9/f39/gICBgIGBgYGAgH+Afn9+fn5+f3+Af4CAgICAgICAgIB/gH9/f39/f4B/gH+Af3+Af4B/gICAgICBgICAgH9/f35/fn9+f39/gICAgIGAgYGAgH+Afn9+fn5+f3+Af4CAgYCAgICAgIB/gH9/f39/f39/gH+Af3+Af4B/gH+AgICBgIGAgIB/f35/fn9+f39/gH+AgIGAgYGBgICAf39+f35+f39/f4CAgYCAgICAgIB/gH+Af39/f39/gH+Af3+Af4B/gH+AgICAgIGAgIB/f39/fn9+f39/f3+AgIGAgYCBgICAf39+f35+fn5/f4B/gYCAgICAgIB/gH+Af39/f39/gH+Af39/f4B/gH+AgICAgIGAgICAf39/fn9+f35/f3+AgIGAgYCBgICAf4B+f35+fn5/f4B/gYCBgICAgIB/gH+Af39/f39/gH+Af39/f4B/gH+AgICAgIGAgYCAf39/fn9+f35/f3+AgIGAgYCBgICAf4B+f35/fn5/f4B/gX+BgICAgIB/gH+Af4B/f39/f39/f39/gH+AgICAgICBgIGAgYCAf35/fX99f35/f3+AgIGAgoCCgIGAgIB+f31/fX5+f39/gX+BgIGAgYCAgH+Af4B/f39/f39/f39/f39/gICAgICAgIGAgYCAgH9/fn99f31/fn+Af4GAgoCCgIGAgIB+gH1/fX9+f39/gH+BgIGAgYCAgH+Af4B/gH9/f39/f39/f39/gH+AgICAgIGAgYCAf39+fn59fn1/foCAgYKCgoGCgYGAf39+f31+fX19fn9+gICBgYKCgYGAgX+Afn9+f39+f35/fn9+f39/gYCBgIGBgYGAgX+Af39+fn59fn1+fn9/gIGBgoGCgYGBgIB+f31+fX59fn5+gH+BgIKBgoKBgYCBf4B+f35+f35/fn9+f39/gICBgIKBgYGBgYCAf39+fn59fn1+fX9/gIGBgoGDgYKBgYB/f31/fX59fn5+f36Bf4KAgoGBgoCBf4F+gH5/f35/fn9+f35/f3+AgIGBgoGBgYGBgIB/fn59fn1+fX5+f4CAgoGDgoOCgYF/gH5/fX58fn1+f36Af4KAgoGBgYCCf4F/gH5/f35/fn9+f35/f3+AgIGAgYGCgYGBgIB/f359fn1+fX5+f4CAgoGDgYOCgoGAgH5/fX58fn1+f36Af4F/goCCgYGCgIF/gX6Af39/fn9+f35/fn9/gICAgYGCgYGBgYCAf35+fn1+fX5+fn9/gYCDgYOCgoGAgX6AfX98fn1+fn6AfoF/goCCgYGBgIF/gX6Afn9/fn9+f35/fn9/f4CAgYGCgYKBgYGAf39+fn1+fH59fn9/gYCCgYOBgoGBgX+AfX98fn1+fn6AfoF/goCCgIGBgIF/gX6Afn9/fn9+f35/fn9/f3+AgYGBgYKBgYGAgH9+fn1+fH59fn5/gICCgYOBg4GBgX+AfX98fnx+fX5/foF/gn+CgIGBgIF/gX+BfoB/f39+f35/fn9+f3+AgICBgYKBgoGBgH9/fn1+fH59fn5/gH+CgIOBg4GCgYCAfn99f3x+fX5/foB/gn+CgIGAgIF/gX+BfoB/f39+f35/fn9+f3+AgICBgYKBgoGBgIB/f35+fX59fn5+gH+CgIOBg4GBgX+BfYB8f3x+fX5/foF/gn+Cf4KAgIF/gX6BfoB+f39/f36AfoB+gH6Af4CAgYGBgoGCgIB/f35+fX19fn5+gH+BgIOBg4GCgYCBfoB8f3x+fX5+foB/gn+Df4KAgYGAgX6BfoF+gH5/f36AfoB+gH6Af4CAgIGBgoGCgIF/gH5+fX59fn5+f3+Bf4OAg4GCgYGBf4B9f3x/fH5+foB/gn+Df4KAgYCAgX+BfoF+gH5+f31/fYB+gH6Af4CBgIKBgoGCgYF/gH5+fX18fX19f3+BgIKBg4KDgoGCf4F9f3x+fH59fX9+gX+DgIOAgoGAgX+BfoF+gH5/fn5/fYB9gH6Af4CAgIGBgoGCgYKAgH5/fX18fX19fn6Af4KBg4GDgoKCgIF+gHx/fH59fX9+gX6Cf4OAgoCBgX+BfoF+gX6Afn9/fn99gH2AfoB/gIGBgoGCgYKAgX9/fX58fX19fn2AfoKAg4GDgoKCgIF+gHx/fH58fn5+gH6CfoN/goCBgX+BfoJ+gX6Afn9/fn99gH2AfYB+gICAgYGCgYOAgn+Afn99fX19fn1/foF/g4CDgoKCgYJ/gX2AfH98fn5+gH6CfoN/gn+BgICBf4J+gX6BfoB/f39+gH2AfYB+gH+AgYGCgYOBgoCBfn99fn19fX1/fYF/goCDgYKCgYJ/gX2AfH98fn1+f36BfoJ/gn+BgICBf4F+gX6BfoB/f39+gH2AfYB9gH6AgIGBgYKBgoCBf4B9fn19fX1+fYB+gn+DgYOCgoKAgn6AfH98fn1+f36BfoJ/g3+CgICAf4F+gX6BfoB+gH9/gH6AfYB9gH6Af4CBgYKBg4CCf4B+f319fX1+fYB+gX+CgIOCgoKAgn6BfYB8f31+fn6AfoJ/g3+Cf4GAf4F+gX6BfoF+gH9/gH6AfYB9gH6Af4CAgYKBg4CCf4F+f31+fX1+fX99gX+CgIOBgoKBgn+BfYB8f3x+fn6AfoJ+gn+Cf4GAgIB/gX6BfoF+gH9/f36AfoB9gH2AfoCAgIGBgoCDgIJ+gH1+fX1+fX99gX6Cf4KBgoKBgn+BfYB8f3x+fn6AfoF+gn+Cf4F/gIB/gX6BfoF+gX+Af3+AfoB9gH2AfoB/gIGBgoGDgIJ/gX5/fX59fX99gH6Cf4KAgoGBgoCCfoF9gHx/fX5/foF+gn+Cf4F/gIB/gH6BfoF+gX+Af3+AfoB+gH2AfYB+gICAgoGCgIJ/gX6AfX59fX59gH2Bf4KAgoGBgoCCfoF9gHx/fX5/foB+gn+Cf4J/gH9/gH6BfoF+gX6Bf4CAf4B+gH2AfYB+gH+AgYGCgIN/gn6AfX99fX59f32BfoJ/goGCgYCCf4F9gHx/fX9+foB+gX+Cf4J/gX+AgH+AfoF+gX6Bf4B/f4B+gH6AfYB+gH+AgIGCgIOAgn+Bfn99fn59f32AfoJ/goCCgYGCf4F+gX2AfX9+fn9+gX+Cf4J/gX+AgH+AfoF+gX6Bf4B/gIB/gH6AfYB9gH6AgIGBgIKAgn+BfoB9fn59f32AfYF+goCCgYGCgIJ+gX2AfX99fn9+gX+Cf4J/gX+Af3+Af4B+gX6Bf4F/gIB/gH6AfYB9gH6Af4GBgIKAgn+CfoB9f359fn2AfYF+gn+CgIGBgIJ+gX2AfX99f39+gH6Bf4J/gX+Af3+Af4B+gX6BfoF/gIB/gH+AfoB9gH6AfoCAgIGAgn+CfoF+f35+f32AfoF/goCCgIGBgIF+gX2AfH99fn5+gH6Bf4J/goCBgICAf4B+gH6AfoB+gH9/f3+AfoB9gH6AfoGAgYKBg4CDfoF9gH1+fn1/fYB9gX6CgIKBgYKAgn+CfoB9f35+f36AfoF+gX+Bf4B/f4B/gH+Bf4J/gYCBgH9/fn99f31/fYB+gICAgoCDf4J/gX6Af35/fYB9gH6Bf4GAgIF/gX6BfoB+gH5/f3+Bf4KAgn+Bf4B/f39+f36AfoF/gX+BgICAf4B/gH6AfoB+gH+AgX+Cf4J+gX5/fn5/fYB9gX6Cf4KBgoGAgn+BfYB9f31+fn6AfoF/gn+BgIGAgICAgH+AfoF+gH6Af39/f4B+gH6AfoF+gYCBgYGDgIN/gn6AfX5+fX98gH2BfoGAgoGBgoCCf4J+gX5/fn5/foB+gH6Af4B/f39/f3+Af4F/gYCBgIGAgIB/gH6AfYB+gH6BgICCgIJ/gn6Bfn9+fn59f32AfYB+gH+AgH+Bf4F+gX+Af4CBgIGAgoCCgIGAgH9/fn5+fX99f36AfoB/gH9/gH+AfoB+gH6Af4CBgIJ/gn+Cf4B/f4B+gH6BfoJ/gYCBgX+BfoB9f3x+fX1+fX9+gH+BgIGAgYCBgICAgIF/gX+Bf4B/gH9/f35/fX99f32AfoCAgIF/gn+CfoF+f39+gH2BfoF/goCBgYGCgIJ/gX6Afn9+fn59f35/fn9/f39/f39/f4CAgYCBgIKBgoGBgICAfoB+gH2AfoCAgIF/gn+CfoF+f35+f31/fX99f36Af4CBf4F/gX+Bf4CAgIGAgYCBgIGAgH9/fn5+fn5+f3+Af4GAgYCBgICAf4B+gH2AfoB/gICAgoCCgIKAgYCAgH6Afn9+f35/f3+AfoB+gH5/fn5/foB+gX+BgIGBgIGAgH9/f39/f3+Af4CAgICAgICBgIF/gX+Af4B/f4B/gX6BfoF+gH5+f31/fYB9gX+BgIGBgIKAgn+Bf4B/f39/gH6Bf4F/gYCAgIB/f39/f35/foB+gH6Af39/f4B+gX6BfoF/gYCBgoGDgIN/gn+Afn5+fX58fnx/fX9/f4B/gX+BgIGAgICAgICAgICAgICAgIB/gH+Af39/f4B/gH+Af4B/f39+fn1+fX59fn5/gICBgIKBg4GCgYGBf4F+gX6Af39/f4B+gH6AfoB+f39+f36Af4B/gICAgH9/f39/f39/f4CAgYCBgYGBgYGAgYCAf4B+f35+fn5/fYB9gH6Af39/foB+gX6Bf4GAgYGBgoCCgIF/gH9/f39/f4B/gICAgICAf39/f39/f39/f35/fn9+gH+AgH+Bf4F/gYCBgIGBgYKAgoCBf4B/fn59f31/fX9+f4B/gX+BgIGAgIB/f39/fn5+fn5+f39/f4CAgIGAgYGBgYGCgYKBgoCBgIB/fn59fnx+fX5+fn9+gH+Bf4CAf4B/gH6Afn9+f3+AgICBgYKBgoGBgYCBgICAgIB/gH5/fn59fX19fXx9fX5+f4CAgYCBgYGBgYGAgYCBf4CAgIB/gYCBgIGAgIB/gH5/fn9+f39+f36AfoB+f35+fn5/fX9+gICBgYGCgoKCgoKBgYCAgH9/fn9+f35/fn5+fn9+f35/fn9/gICAgYCCgIKAgYB/f35/fX9+f3+AgICBgIKAgYCBgICAf39+fn5+fn5+fn9/f39/gH+AgICAgIGAgoGCgYGBgIB/gH5/fX99f35/gH+Bf4KAgYCAf39/fn99fn1+fn9/f4CAgIGBgYCBgICAf4B/gH+Af4B/f39/f35/fYB+gH+AgICCgIKAgoCBgIB/fn9+f35/fn9/gICAgICAgH9/f39/f39/gH+AgIGAgYCAgH+AfoB+f35/f3+Af4F/gYCBgICAf4B/gH+Af4B/gICAgICAgIB/f39/f39/f3+Af4GAgX+Af35/fX99f31/f4CAgIKBgoKCgoGBgIF/gH9/f39/f39/f39/f39/f39+f35/f39/f4B/gYCBgICAf4B+gH6Af4CAgIGAgoCBf4B/f35+fn1+fn9+gICBgIGBgoGBgYGAgIB/f35/foB/gH+Af39/fn99f31/fn9/f4F/gX+Bf4F/gIB/gH6AfoB/gYCBgYGBgYGAgH9/fn9+fn5+fn9/f4B/gH+Af4B+f35/fn9/f4B/gYCCgYKBgYGAgX+AfoB+f39/gH+Af4B+gH5/fn5+fn9+f3+AgICAgYCAgIB/f39/f3+AgIGAgYGCgYKBgYB/f35+fX19fX1+fn5/f4CAgICAgICAgICAgICAgYCBgIGAgIB/gH5/fn9/gH+AgICBgIF/gX+Afn5+fX59fn1/foB/gICBgYCAgIB/gH9/foB/gICAgYCBgIF/gX+Bf4B/f4B/gH6BfoF+gX+Af35/fX99f32AfoB/gICAgYCBf4F/gH+Af4CAgIGAgYCCgIF/gH9/fn5+fX99gH6Bf4GAgYCAgH+AfoB9f31/fn9/f4F/gYCCgIGAgYCAgYCBgIGAgYCAf39/fn59fn1+fX9+f3+AgIGBgYGAgYCAf39+f35/f4CAgIGBgoGCgYGBgIB/f39/f39/fn9+f35/fn5+fn5+f35/f4CAgYGBgoGCgIGAgH9/f36Af4B/gYCCgYGBgYF/gH5/fX99fn1+fn9/f4B/gH6AfoB+gH+AgICBgIKAgoCCgIGAgIB+gH2AfYF+gX+AgH+BfoF+gH2AfX9+f39/gH+Bf4J/gX+Af39+fn99f36Af4GAgoGCgYGCgIF+gX2AfX99fn9+gH6BfoF+gX6Af39/foB+gX6Bf4F/gIB/gH+AfoB/gH+BgIGBgoKBgYGBf39+fn19fH19fn5+gH+BgIGAgYCAgH+Af4B/gH+Af3+Af4B/f39/f39/f4B/gICBgYCBgIF/gH9/fn5+fn9+gH6Bf4GAgYGAgYCBf4B+f35/f39/f4CAgH+Bf4B/gH5/fn9/f4B/gX+Cf4J/gX+Af36AfYB9gX2Bf4CAgIF/gn6CfoF/gH+AgX+Cf4J/gn+Bf39+fn59fnx+fH99gH+BgIKBgYKAgn6BfYF9gH5/f3+BfoJ+gn6Cf4F/f4B+gX6BfoF+gH9/f35/fX99f32Af4CAgYKBg4KDgYKAgX9/fX59fX19fn6AfoF/gYCBgICAf4B+gH6Afn9/f4B/gH+Af4B/gIB/gX+BgIGBgYGAgX+Afn9+fn59fn1/foB/gICBgYCBgIB/gH5/fn9/f4CAgICBgIGAgYCBf4B/gH9/gH+BfoF+gX6Bf39/foB9gHyAfYB+gIB/gX+CfoJ+gX6Af4CAf4F/gn+Cf4J/gH9/f35/fYB9gH2Bf4GAgYGBgn+CfoF9gX2AfX9/foF+gn6DfoJ/gYCAgX+CfoJ9gX6Afn9+fX98f3yAfYB+gICBgoGDgYOBgoCBf4B+fn19fn1/foB+gX+CgIGBgYF/gX6AfoB+f35+f35/foB+gH+AgICBgIKBg4KDgoKCgIF+f31+fHx8e318fn2Af4GBgYGBgoCBgIGAgYCBgYGBgYGBgYCAf39+fn19fX1+fX99gH6BfoB/gIB/gH6BfYJ+g4CDgoGDgIN+g32BfYB9fn59gH2BfYF9gX6Af39/fX99gH2BfoKAg4GCgoGCf4F9gXx/e358fX19f32BfYJ+g4CCgYGCgIN/hH+Ef4KAgIB9gHyAe397f3x+fn6Af4J/goCCf4F/gH5+fn5/foB/goCDgYOBgoGBgX+BfoB9f31+fX1+fX99f31/fn9/f4GAgoGEgoOCgoKAgX+AfX99fX18fX1/foB/gYCBgYGBgIF/gH+Af4B/gICAgICAf4B/gH5/fn5+fn5+gH6Bf4F/gYCBgX+BfoJ+gn6DgIKCgYOAg36DfYF9f319fnx/fIB8gH2AfYB/f4B+gH6BfoKAg4GDgoODgoOAgn6BfYB8fnx9fXx/fIF9gn6Cf4GAgIJ/g36DfoN/gX9/gH6AfIB8gHx/fX9/f4F/gn+Df4J/gX+Afn5+fX99gH6Bf4KAgoGBgYCBf4F+gH1/fX59fX59f32AfoB/gYCAgoCDgYOBg4GBgYCBfoB9f3x9fH19fX5+gH+BgIGBgYGAgYCAf4B/gICAgICAgIB/gH+Afn9+fn5+fn1/foB+gX+BgIGBgIF/gX6CfoJ/gYGAgX+CfYF9gX1/fX5+fX99gH2BfYF+gH9/gH+BfoF/goCDgYOCg4KBgoCCfoF8f3t9e3x8e358gHyBfoJ/gYGBgoCDf4R/g3+Cf4B/foB8gHyAfIB9gH+AgYCCgIN/gn+Bfn9+fn59fn2AfoF/goCCgYKCgYJ/gn6BfYB9fn19fn1/fYB9gH2BfoGAgYKBg4GEgYOBgoCBf39+fX18fX19fn5/f4CAgIGAgX+AfoB/gH+AgICBgYKAgoCCf4F/gH5/fn5/fn9+gH+Af4F+gH5/fn5/foB+gX+BgIGBgYGAgYCBf4B/f39/gH6AfoB+gH5/f35/fYB+gX+CgIOBg4KCg4GDf4J9gHx+e3x7fH18fn2AfoJ+g3+DgIKBgYOAg3+Df4J/gX5/fn5/fH98f3x/fn9/gIF/gX+Bf4B+f35+f3+BgIOAg4GDgoOCgYJ/gX2AfH97fnp+e318fX59gH2BfoOAhIKFhIWEhISCg4CBfn98fXt7e3t7fHt9fX9+gICBgYCCgIKAg4GDgYOCg4GCgYCBf4B+f319fH18fXx9fH59fn5/f36AfoF/gYGCgoODg4ODgoKCgYCAfn59fn19fn1+fH98f31/fX5/foB+gX+DgIWBhYKEgoOCgYJ+gHx+en16fHp9e318fn9/gX+DgISBhIOFhISEg4OBgn+AfX58fHt7fHt9fH59fn9+gX+Bf4F/gH9/f3+Af4KAg4GEgoSCg4KCgoCBfn99f3t+en16fXp8fHx+fYB+goCEgoWDhoSFhIODgIJ+f3x9e3x7fHx9fX5+f3+AgICBgIF/gYCBgIKBgoGCgYGBgYGAgH9/fn59fn1+fX59fn1/fn9/f4B/gYCBgYKCg4KDgoKBgYB/f35+fX19fX1+fX99gH2AfoB/f39/gH+BgIOBhIGFgoSCg4KBgX5/fH57fHp8enx7fHx9fn6Af4KAg4GEg4WEhYSEg4KCgIB+fnx9e3t8e318fn5/f4CBgIGAgYCAf39/f4B/gYCCgYOBg4GDgYKAgH9+fn19fH17fXt9e318fX5+gH+CgYSChoSGhIWEg4KAgH1+e3x6e3p7e3x9fn5/f4CAgIGAgYCBgIGBgoGCgYKBgoGBgYCAf35+fX19fX19fX1+fX99f35/f3+AgIKBg4KEg4SDg4KCgYCAfn59fXx9fX19fn5/fn9+f35+fn5/foB/goGDgoSDhIODg4KCgIB+fnx9fHx7fXx9fH59fn5/gH+CgIOChIKEg4ODgoKAgX5/fH57fXx9fX1/foB/gX+Bf4B/f39/f39/f4CAgoGDgoOCgoKCgYCAf39+fX19fXx8fHx8fHx9fn6Af4KBhIOFhYaGhYWDg4CBfX57fHp7ent7fH19fn1/foB/gYCBgIGBgYKCg4KEgoSCg4GBgIB/fn59fX1+fX59fn1+fX19fn5+fn+AgYGCg4OEhISDg4KCgIB/fn19fHx9fX19fn1+fX59fn5/fn9/gIGCg4OFhYWEhYODgYF/f319fHt7e3t7fHt8fH18fn6Af4GBgoOEhIWFhYWDhIGCf399fXx8e3x8fX1+fn9/f39/f39/f39/gH+BgYKCg4ODg4KCgoGBgH9+fn19fXt9e317fXt9fH1+foCAgoKFg4eFh4WGhYSDgIB+fnt8ent5e3p7e3x8fX5+f3+AgIGAgoGDgoSDhISEhIKDgYGAf35+fX19fX19fX18fXx9fX19fn5+gICCgoSDhYSFhISDgoKAgH5+fX18fHx8fH18fX19fX1+fn9/gICBgoODhYWGhYWEg4KBgH9+fX18fHt8e3x7fHt9fH1+fn9/gYCDgoSDhYSFhISDgoF/gH1+fH18fHx9fX5+fn9+f35/fn9/f39/gIGCgoODg4ODg4KCgYF/gH5+fX19fH18fXt9e318fn1/f4CBgoSDhoWGhYaEhIKBgH5+fHx6e3p8e3x8fX1+fn5/f4CAgICBgYKDg4ODhIODgoKBgYB/fn19fX18fX19fX19fX19fn1+foCAgYKChISFhIWEhIOCgYB/fn59fXx8fHx8fX19fX1+fn5+f3+AgYGCg4SEhIWEhIODgYGAf359fHx8fHx7fHx9fH19fn5/f4CBgYKChISFhIWEhIOCgYB/fn58fXx8fH19fX59fn5/fn9+f39/gH+BgIKCg4ODhIODgoKBgX9/fn59fX18fXx9fH18fX1+fn9/gIGBg4OFhIaEhYODgoGAfn58fXt8e3x7fH19fn1/fn9/gH+AgIGBgYKDg4OEg4ODgoGBgH9/fX59fXx9fX19fX59fn1+fn9+f4CAgYKDg4SDhIOEgoKBgIB+fn19fH18fX19fX5+fn5+fn5/f3+AgYGCg4SEhISEg4KBgYB/fn19fHx8fHx9fH19fn1+fn+AgIGBgoKEg4SDhIODg4GBf399fnx9e3x8fH19fn1+fn9+f35/f4CAgIGCg4OEhISEg4OCgYCAfn59fXx8fHx8fH18fXx+fX5+f4CAgoGEgoWEhoSFhIOCgYF+f3x9e317fHt9fH19fn5+f36Af4B/gYCCgYODg4ODg4KCgIB/f359fX18fH19fX59fn5/fn9/f4CAgYCCgoODhIOEg4OCgYF/f35+fHx8fHx8fH19fX1+fn5+f39/gICCgoSDhYSFhISDgoKAgH5+fX18fHt8e318fX1+fn5/foB/gX+CgYSChIOEhIODgYJ/gX5/fH18fXx9fX19fX5+fn5/fn9/gICBgYKCg4OEhIODgoKBgH9+fn19fHx8fH18fnx+fX9+f39/gYCCgISBhYOFg4SDgoKAgX5/fH17fHt8e318fX19fn5/foB/gX+CgIOChIOEhIOEgoOAgX9/fX18fHx8fH19fn1+fn5+fn9+gH+Bf4KAg4KDg4ODgoOBgX+Afn59fXx9fH19fX1+fX5+fn9/gH+BgIOBhIOEhISEg4OBgX9/fX18fHt8e318fXx+fX5+f4B/gX+CgIOBhIKEg4OEgoOAgn+AfX99fXx9fH19fX1+fn5+fn9+f36Af4GBg4KDg4SEg4OCgoCAf359fXx9fH18fnx+fX9+f35+gH6Bf4KAg4GEgoSDg4OBgoCBfn99fnx9fH18fX19fX5+fn9+gH+Bf4KAg4GEg4ODg4OBgn+Afn99fXx9fH18fX1+fn9+f39/gH+Bf4J/goCDgoODg4OCg4CBf4B+f319fH18fXx9fH59fn5+f36Af4KAg4GEgoSEhISDg4GCf4B9fnx9e317fXx+fX5+fn5+f36AfoJ/g4CDgYODg4OCg4CCf4F+f31+fX59fX1+fX5+fn5+f36AfoF/goCDgYOCg4OCgoGBf4B+fn1+fH18fnx+fX9+f35/f3+AfoF+g3+DgISCg4OCg4GCf4F+f3x+fH18fXx9fH19fn5+f36BfoJ/g4CEgoSDg4SCg4GCf4B+f3x9fH18fXx9fX5+f39/f3+Af4F/gn+CgIOBg4KCg4GCgIF/gH5/fX58fnx+fH59fn1+fn6AfoF+g3+EgISChIODg4GDgIF+gHx+e317fXt9fH59f35/gH+Bf4J/gn+DgIOBg4KCg4GDgIJ+gH1+fH18fXx9fH19fn1+fn5/foB/gn+DgISBhIODg4KDgYJ/gH1/fH58fXx9fH58f31/fn9/foB+gn6Df4SAhIKDg4KDgYN/gX6AfX98fnx+fH58fn1+fn5/foF+gn6Df4SAhIKDg4KDgIJ/gX1/fH58fnx+fX5+f35/f3+Af4F+gX6Cf4KAg4GCgoKCgYKAgX6AfX58fnx+fH58fnx/fX9/f4B+gn6Df4SAhYKEg4ODgYN/gX2AfH57fnt9e318fn1+fn6AfoF+gX6CfoOAg4GDgoKDgYOAgn+Bfn99fnx+fH18fn1+fX9+f4B+gX6Cf4OAhIGEgoOCgYKAgX6AfX98fnx+fH58fn1/fn9/f4B+gX6CfoN/g4CDgYODgoOAgn+BfoB9f3x+fH58fnx+fX5+foB+gX6DfoR/hICEgoODgYOAgn6BfX98fnx9fH19fn1/fn9/f4B/gX6CfoJ/goCDgYKCgoKBgoCBf4B+f31+fH58fnx/fH99f39+gX6CfoN+hH+EgYOCgoOAgn6BfYB8f3t+fH58fn1/fn9/f4B/gX6CfoJ+gn+CgIKCgYKAgn+BfoB9f31+fH58fnx+fX9+f39/gX+CfoN/hICEgYOCgoKBgn+BfYB8f3x+e358fnx+fX9+f4B/gX+CfoN+g3+DgIOBgoKBgoCCf4F+gH1+fH57fnt+fH59fn5+gH6CfoN+hH+EgISBg4KBg4CCfoF9gHx/fH58fn1+fX9+f39/gH+BfoJ+gn6Cf4KAgoGBgoCBf4F+gH1/fH58fnx/fH99gH6AgH+Cf4N+hH+Ef4SBg4KCgoCCfoF9gHx/e357fnx+fH5+f39/gH6BfoJ+g3+DgIOBgoKCg4GDgIJ/gX5/fX58fnx+fH58f31/f36AfoJ+g36Df4OAg4GCgoCCf4F+gH1/fH98f3x/fX9+gH+AgICBf4J+g36DfoN/goCBgYCBf4F+gH1/fH98fnt+fH98f35/f3+Bf4N+hH6Ef4SAg4KCgoGDf4J+gX2AfH58fXx9fH19fn5/f3+Af4F/gn6Df4OAgoGCgoGCgYKAgX6AfYB8f3x/fH98f31/f3+AfoJ+g32EfYN+g4CCgYGCf4J+gn2BfIB8f3x+fX5+f39/gH+Bf4J/g3+Df4N/goCCgYGBgIF/gX6AfX98fnx+fH58f31/foCAf4J/g36EfoR/hICDgYGCgIJ/gX2BfIB8f3t+e358fn1/f3+Af4F/gn6DfoN/g4CCgYKCgYKAgn+BfYB9f3x/fH58f31/fn+AfoF+g36DfoN+g4CCgYGCgIJ/gn6BfYB8f3x+fH59fn5/f3+Af4F/gn+CfoN/gn+CgIKBgYGAgX+BfoB9gHx/e397f3yAfoB/f4F/g36EfYR+hH6DgIKBgIJ/gn2CfIF8gHx/fH59fn5+f3+Af4F/gn+Cf4N/goCCgYGBgYKAgX+BfoB+f31+fH58f3x/fYB+gIB/gn+DfoR+hH6Df4KAgYF/gX6CfYF9gHyAfH98f31/fn9/f4F/gn6CfoN+g3+CgIKBgYGAgX+BfoF9gH1/fH98f3x/fX9/f4B/gn+DfoN+g36Df4KAgYGAgn+BfoF9gHx/fH58fn1/fn9/gICAgYCCf4N/g3+Cf4KAgYCAgH+AfoB9f3x/fH97f3yAfYB+gICAgn+DfoR+hH6Ef4OAgYGAgn+CfoF9gXyAfH98fnx+fn5/foB/gX+Cf4N/g3+CgIKAgYGBgYCBf4B+f31/fH98f3x/fIB+gH+AgYCCf4N+hH6EfoN/goCBgX+BfoF9gXyAfIB8f3x/fX9+f4B/gX+CfoN+g36Df4KAgYGBgYCBf4F+gH2AfH98f3x/fH99f39/gH+Cf4N+hH6Df4N/goCBgYCBf4F+gX2AfH98f3x/fX9+f39/gX+Cf4J/g3+Df4J/gYCBgICAf4B/gH6AfYB8f3x/fIB9gH6AgH+Cf4N+hH6EfoN+goCBgYCCf4J+gn2BfIB8f3x/fX5+fn9+gH+Bf4J/g3+Cf4KAgYCBgYCBgIF/gH6Afn99f3x/fIB9gH6Af4CBgIJ/g36DfoN+gn6Bf4CAf4F+gX2BfIF8gHyAfX9+f39/gX+Cf4N/g3+Df4J/gYCAgICBf4F/gH6AfYB9f3x/fH99f36Af4CBf4J/g3+DfoN+gn+BgICAf4F/gX6BfYB9gHx/fH99f35/gH+Bf4J/gn+Df4J/gn+BgIGAgIB/gH+AfoB9gHyAfIB8gH6Af4CAf4J/g36DfoN+gn6Bf4CAf4F/gX6BfYF9gHx/fH99fn5+gH6Bf4J/gn+Cf4J/gX+BgICAgICAgH+Af4B+gH2AfIB8gH2AfoCAgIF/gn+DfoN+gn6Cf4F/gIB/gX6BfYF8gHyAfH99f35/f3+Bf4J/g3+Df4J/goCBgICAgIF/gX+Bf4B+gH1/fH98f31/fn9/f4B/gn6CfoJ+gn6Cf4GAgIGAgX+CfoF+gX2AfH99f35/f3+Af4F/gn+Cf4J/gn+Bf4CAgICAgH+Af4B+gH2AfIB8gH2AfoB/f4F/gn+DfoN+gn6Cf4F/gIB/gX+BfoF+gX2AfYB9f35/f36Af4F/gn+Cf4KAgYCBgICAgICAgICAf4B/gH6AfYB8gH2AfoB/gICAgX+Cf4N+g36CfoF+gH9/gH+AfoF9gXyBfIB8gH1/f3+Af4F/gn+Df4J/gn+Bf4B/gIB/gH+Af4B/gH6AfYB9gH2AfoB/gIB/gX+Cf4J/gn+Cf4F/gYCAgH+Bf4F+gX6BfYB9gH1/fn9/f4B/gX+Cf4J/gn+Bf4F/gH+Af39/f4B/gH6AfYB9gH2AfoB/gICAgX+Cf4N/gn6CfoF/gX+AgH+AfoB+gX6AfYB9gH1/fn9/f4B/gX+Cf4KAgYCBgICAgICAf4B/f39/f39/foB9gH2AfYB+gH+AgICBf4J/gn+CfoJ/gX+Af4CAf4B+gX6BfYF9gH2Afn9/f4B/gX+Cf4J/gn+Bf4B/gH9/gH+Af4B/gH+AfoB+gH2AfoB+gH+AgICBf4J/gn+Cf4F/gH+Af39/f4B/gH6AfoB9gH2AfoB+f39/gX+Cf4J/gn+BgIGAgH+Af39/f39/f39/foB+gH2BfYF+gH+AgICBf4J/gn+Cf4F/gH9/f39/f4B+gH6AfoF+gX6AfoB/gIB/gX+Bf4J/gX+BgICAf4B/f39/f39/f39/f4B+gH6AfYB+gH6AgICBgIJ/gn+Cf4F/gX+Af39/f39+f36AfoB+gX6BfoF+gICAgX+Bf4J/gn+Bf4B/f39/f39/f39/f39/f4B/gH6BfoF+gH+AgICBf4F/gn+Bf4F/gH9/f39/f39/f3+Af4B/gH6BfoF+gH+AgH+Af4F/gX+BgIGAgICAf39/f35/fn9/f39/gH6BfoF+gX6Af4CAf4F/gn+Cf4F/gH+Af39/f39/gH+Af4B/gX+BfoB/gH9/gH+Af4F+gX+Bf4CAf4B/gH9/f3+Af4B/gH+AgH+Bf4F+gX6Bf4CAgIB/gX+Bf4F/gH+Af39/f39/f39/f4B/gH6BfoF+gX+AgICBf4F/gn+Bf4GAgIB/f39/f39/fn9/f39/gH+Af4F/gX+Bf4CAf4B/gX+Bf4F/gH9/f39/f39/f39/f4B/gH+Af4B/gH+Af4CAf4B/gH+BgICAgICAgH9/f39/fn9+f39/f3+Af4F+gX6Bf4GAgIB/gX+Bf4F/gH+Af39/f39/f39/f4B/gH+Af4B/gH+Af3+Af4B/gH+Bf4CAgIB/gH+Af39/f4B+gH+Af4CAf4B/gX6Bf4F/gIB/gH+Bf4F/gH+Af39/f39/f39/f39/gH+Af4B/gH+Af4B/gIB/gX+Bf4GAgICAgH9/f39/fn9+f35/f3+Af4B/gX+Bf4GAgICAgX+Bf4F/gH+Af3+Af4B/gH9/gH+Af39/f39/gH+Af4B/f4B/gH+BgIGAgICAgH+Af39/f39+f35/fn9/f4B/gX+Bf4GAgYGAgX+Bf4F/gH+Af39/fn9+f35/f39/f39/f4B/gICAgICAgICAgX+BgIGAgICAgH+Af4B+f39+f35/fn9+f39/gH+Bf4GAgYCBgYCBf4F/gH+Af39/f39+f35/f39/f39/f4B/gH+AgICAgICAgYCBf4GAgYCAgH+AfoB+f35/fn5/fn9+f39/gICBgIGAgYGBgYCBf4F/gH9/f35/foB9gH6Afn9/f4B/gH+Af4CAgICAgICAgICAgICAgICAgH+Af4B+f35/fn5/fn9+f39/f3+AgIGAgYGBgYCBgIF/gX+Af39/fn99gH2AfoB/f39/gH+Af4B/gH+AgICAgICAgICAgICAgH+Af4B/gH5/f35/fn9+f36Af4CAgIGAgYCBgIGBgIF/gH+Af39/f39+f35/fn9/f39/gH+Af4B/gICAgICAgICAgICAgICAgICAf4B+gH5/fn9/fn9+f36Af4CAgIGAgYGBgYGBgIB/gH9/f39/fn9+f35/fn9/f4B/gH+Af4B/gH+AgH+Af4CAgICAgICAgICAgIB/gH9/f35/fn9+f35/f3+AgIGAgYCBgIGBgIGAgX+Af4B/f39+f36AfoB/gH9/gH+Af4B/gH+Af4CAgICAgICAgICAgH+Bf4B/gH5/fn9/fn9+gH6AfoB/gICAgYCBgIGAgYCAgICAf39/f39+f35/fn9/f39/gH+Af4B/gH+Af4CAgICAgICAgICAgICAgIB/gH6Afn9+f35+f36AfoB/gICBgIGBgYGBgICAgICAf39/fn9+f36AfoB+gH9/gH+Bf4F/gX+Af4B/f4B/gICAgIGAgYCAgIB/gH9/fn9+fn5+f35/fn9/gICAgICBgYGBgYGAgYCAf4B/f39/f36AfoB+gH6Af3+Af4B/gH+Af4B/gH+Af4CAgICAgICBgIF/gX+AfoB+f35/f35/foB/gH+Af4GAgYCBgICAgICAf4B/f39+f35/foB+gH+Af4CAgIB/gH+Af4B/gH+Af4CAgICBgICAgICAgH+AfoB+f35/fn9+f39/gH+Af4F/gYCBgIGAgYCAgICAf4B+gH6AfYB+gH6Af4CAgIF/gX+BfoB+gH+Af4CAgICAgICBgIGAgH+AfoB+f35/fn9+f39/gH+AgIGAgYCCgIKAgYCBf4B/f39+f36AfYB9gH6Af4CAgIB/gX+BfoF+gX6Af4B/gICAgICBgIF/gX+BfoB+f35/fn9/f39/gH+Af4F/gYCBgIGAgYCAgH+Af4B+gH6AfoB+gH6Af4CAgICAgX+Bf4F+gX6Af4B/gH+AgICAgIF/gX+AfoB+gH5/fn9+f39/gH+Bf4F/gn+CgIKAgYCAgH+Af4B+gH2AfYB9gH6Af4CAgIGAgX+Bf4F+gX6AfoB/gH9/gH+AgIGAgX+Bf4B+gH5/fn9+f39/f3+Af4F/gX+Cf4KAgX+Bf4B/f39+gH6AfYB9gH2AfoB/gICAgYCBf4F+gX6BfoB/gH+AgH+Af4F/gX+Bf4F+gH5/fn9+f39/f3+Af4F/gX+Bf4GAgYCBgICAf4B/gH6AfoB9gH2AfoB/gICAgYCBf4F/gX6BfoB+gH+Af3+Af4F/gX+Bf4F+gH5/fn9+f39/f3+Af4F/gX+Cf4J/gX+Bf4CAf4B/gH6AfYB9gH2AfoB/gICAgH+Bf4F/gX6BfoB+gH+AgICAgIGAgYCBf4F/gH5/fn9+f35/f39/f4B/gX+Bf4J/gn+Bf4GAgIB/gH6AfoB9gH2AfYB+gH+AgICBf4F/gX6BfoF+gH+Af4CAgIGAgX+Bf4F/gH6Afn9+f35/f39/f4B/gX+Bf4F/gX+Bf4GAgIB/gH+AfoB9gH2AfoB+gH+AgICBgIF/gX+BfoF+gH6Af4B/gICAgYCBf4F/gH6AfX99f35/fn9/f4CAgYCBf4J/gn+Cf4F/gH+Af3+AfoB9gH2AfYB+gH+AgICBgIGAgX+Bf4F+gH6Af4B/gICAgICBf4F/gH6AfoB+f35/fn9/f4CAgICBgIF/gX+Bf4F/gH+Af39/foB+gH2AfYB+gH6Bf4CAgIGAgX+Bf4F+gX6AfoB/gH+AgH+Af4F/gH6AfoB9f31/fn9/f4B/gYCBgIJ/gn+Cf4F/gX+Af39/foB+gH2AfYB9gH6Af4CAgIGAgYCBf4F+gX6AfoB+gH+AgICAf4F/gX6BfoB9gH2Afn9/f4B/gICBf4J/gn+Cf4F/gX+Af39/foB+gH2AfYB9gH6Af4CAgIGAgYCCf4F/gX6BfoB+gH9/f3+Af4B/gX+AfoB+gH5/fn9/f39/gICBgIGAgX+Bf4F/gX+Af4B/f4B+gH6AfYB9gH6AfoB/gICAgYCBgIF/gX+BfoB+gH+Af4CAf4B/gX+BfoB+gH6Afn9+f39/gH+Bf4F/gn+Cf4F/gX+Af39/f4B+gH6AfoB+gH6Af4B/gICAgYCBgIF/gX+Bf4B+gH5/f39/f4B/gH+Af4B+gH6Afn9+f35/f4CAgIGAgn+Cf4J/gX+Af4B/f39+gH6AfYB9gH6AfoB/gICAgYCBgIF/gX+BfoB+gH5/fn9/f4B/gH+Af4B/gH6AfoB+gH+Af4CAgIGAgX+Cf4F/gX+Af4B/f39/gH6AfoB9gH2AfoB+gH+AgICBgIGAgX+Bf4F/gH6Af39/f4B/gH+Af4B+gH6Afn9+f35/f4CAgIGAgn+Cf4J/gX+Bf4B/f4B/gH6AfoB+gH6AfoB/f39/gICBgIGAgX+Bf4F/gH+Af4B/gICAgH+Af4B/gH6Afn9+f35/f4B/gICAgYCCgIJ/gn+Bf4B/gH9/f36AfoB9gH2AfoB+gH+AgICBgIGAgn+Bf4F/gH+Af39/f4B/gH+Af4B/gH6Afn9+f35/f39/f4B/gX+Cf4J/gX+Bf4B/gH9/gH+AfoB+gH2AfoB+gH+Af4CAgIGAgYCBf4F/gX+Afn9/f39/gH+Af4B/gH6AfoB+gH6Af4CAgIGAgX+Cf4J/gX+Bf4B/f39/f36AfoB+gH6AfoB+gH+Af4CAgIGAgYCBf4F/gX+Af4B/gH9/f3+Af4B/gH6AfoB+gH6AfoB/gICAgYCBgIJ/gn+Bf4F+gH5/f39/foB+gH6AfoB+gH+Af4CAgIGAgYCBf4F/gX+AfoB+f39/f3+Af4B/gH+AfoB+gH6AfoB/gH+AgICBgIGAgn+Bf4F/gH+Af39/fn9+f36AfYB+gH6Af4CAgICAgYCBgIGAgX+Bf4B+f35/f39/f4B/gH+AfoB+gH6AfoB/gICAgICBf4F/gX+Bf4F/gH9/f39/f4B+gH6AfoB+gH6Af4B/gICAgICBgIF/gX+Bf4B/gH9/f39/f39/gH+AfoB+gH6AfoB+gH+AgICBgIGAgoCCf4F/gX+Af39/fn9+f35/fn9+f36Af4CAgICAgYCBgIGAgX+Af4B/gH9/f39/f4B/gH+Af4B/gH6AfoB+f39/f3+Af4F/gX+Bf4F/gX+Af39/f39/f35/fn9+f35/foB/gICAgICBgIGAgYCBf4B/gH9/fn9/f39/f3+Af4B/gH6AfoB/gH+AgICAgIGAgX+Bf4F/gH+Af39/fn9+gH6AfoB+gH9/f39/gICAgICAgIGAgYCBgIB/gH+Af39/f39/f39/f4B/gH6AfoB+gH6Af4CAgIGAgoCCgIJ/gX+Af39/f39+f35/fn9+gH6AfoB/gICAgICBgIGAgYCBf4B/gH9/f39/f39/f3+Af4B/gH6AfoB+gH6Af4CAgIGAgYCCf4J/gX+Bf4B/f39+f35/fn9+f35/fn9/f3+AgICBgIGAgYCBgIF/gH+Af39/f39/f39/f4B/gH6AfoB+gH6Af4CAgICAgX+Cf4J/gX+Bf4B/f39+gH6AfoB+gH5/fn9/f39/gH+AgIGAgYCBgICAgICAf39/f39/f39/f39/gH+AfoB+gH6AfoB/gICAgYCBgIJ/gn+Bf4B/f39/f35/foB+gH6AfoB+gH+AgICAgIGAgYCBgIGAgH+Af39/f39/f39/f4B/gH+Af4B+gH6Af39/f4B/gX+Bf4F/gYCBgICAgIB/gH6Afn9+f35/fn9+f39/f3+AgIGAgYCBgIGAgYCAf4B/f39/f39/f39/f3+AfoB+gH6Af4B/gICAgX+Bf4J/gn+Bf4F/gH9/gH6AfoB+f35/fn9+f39/f3+Af4GAgYCBgIGAgYCAgICAf39/f39/f39/f39/f39+gH6AfoB/gH+AgICBgIGAgoCCgIF/gH+Af39/fn9+f35/fn9+gH6Af4CAgICAgYCBgIGAgYCBf4B/f39/f39/f39/f39/f4B+gH6AfoB/f39/gH+Bf4GAgoCBgIGAgIB/gH9/fn9+f35/fn9+f35/f39/gICAgICBgIGAgYCBgICAgH9/f39/f39/f39/fn9+gH6AfoB/gH+AgICBgIGAgoCCgIGAgX+Af39/fn9+f35/fn9+f35/f4B/gICAgICBgIGAgYCBgICAgIB/f39/f39/f39/f39/f39/foB+gH+Af4CAgIGAgYCCgIGAgYCAf39/f39+f35/fn9+f35/foB/gICAgICBgIGAgYCBgIGAgH9/f39/f39/f39/f39/gH6AfoB/gH+Af3+Af4F/gX+BgIGAgYCAgICAf39/f35/fn9+f35/fn9/f3+AgICAgIGAgYCBgIGAgICAf39/f39/f39/fn9+f35/foB+gH+Af4CAgIGAgoCCgIKAgYCBgIB/f39+f35/fn9+f35/fn9/f39/gH+Bf4GAgYCBgIGAgICAgH+Af4B/f39/f39/f35/fn9+f35/f4B/gICAgYCCgIKAgoCBgICAf39/f35/fn99f31/fn9+gH+AgICAgIGAgYCBgIGAgYCAf39/f39/f39/f39/f35/foB+gH5/f39/f4B/gX+Bf4GAgYCBgICAgIB/gH5/fn9+f35/fn9+f39/f3+AgIGAgYCBgIGAgICAgICAf39/f39/f39/f35/fn9+gH6AfoB/gICAgYCBgIKAgoCBgIGAgH9/f35/fn9+f35/fn9+f3+Af4CAgICAgYCBgIGAgICAgICAgIB/f39/f39/f39/f39+f35/fn9/f3+AgICBgIGAgoCBgIGAgICAf39/fn9+fn1/fX9+f36Af4CAgIGAgYCBgIGAgYCBgICAgH9/f39/f39+f35/fn9+f36Af39/f39/gH+Bf4GAgYCBgIGAgYCAgH+Af39+f35/fn9+f35/f39/gICAgYCBgIGAgYCBgICAgICAf39/f39/fn5+fn9+f36AfoB/gH+AgICBgIGAgoCCgIGAgYCAgH9/fn9+f31/fX9+f35/f39/f4CAgYCBgIGAgYCBgICAgICAgH+Af39/f39/f39+f35/fn9+f39/f4CAgIGAgoCCgIKAgYCAgIB/f39+fn1+fX59fn1/fn9/gICAgYCBgIKAgoCBgIGAgICAgH9/f39+f35/fn9+f35/fn9/f39/gH+Af4F/gYCBgIGAgYCAgICAf4B+f35/fn5+fn5/fn9/f3+AgICBgIGAgYCBgIGAgYCAgIB/f39/f35+fn9+f35/foB+gH+AgICAgIGAgoCCgIGAgYCAgICAf4B+f35/fn9+f35/fn9/f3+AgICBgIGAgYCBgIGAgICAgICAf39/f39/fn5+fn5/fn9+f36Af4CAgIGAgYCCgIKAgYCBgICAgH9/f35/fn59fn1/fn9+gH+AgICBgIGAgoCCgIGAgYCAgICAf39/f35/fn9+f35/fn9+gH+Af4CAf4F/gX+BgIGAgYCBgYCAf4B/gH5/fn9+fn5+fn5+f39/f4CAgIGAgYGBgYGBgYGBgICAgH9/f39+fn5+fn1/fX9+gH6Af4CAgIGAgYCCgIKAgoCBgICAgIB/gH5/fn9+f31/fn9+f35/f3+AgIGAgYCBgIGAgYGBgYCAgIB/f39/f35+fn5+fn5+f35/f4B/gICAgYCBgIGAgoCBgIGAgICAgH9/fn9+fn1+fX59f35/f4CAgICAgYCBgIKAgYCBgICAgIB/f39/fn9+f35/fn9+f35/f4CAgIB/gX+Bf4GAgYCBgIGBgIGAgH+Af39+fn5+fn5+fn5/fn9/gICAgYCBgYGBgYGBgIGAgICAf39/f35+fn5+fX59f36AfoB/gICAgYCBgIJ/gn+BgIGAgICAgH+Af4B+f35/fn9+f35/fn9/f4CAgICBgIGAgYCBgIGAgICAgH9/f39/fn5+fn5+f35/fn9/gH+AgICBgIGAgoCCgIGAgYCAgIB/f39+fn5+fX59fn1/fn9/gICAgICBgIKAgoCCgIGAgYCAgH9/f39+f35/fn9+f35/fn9/gH+AgICBgIGAgYCBgIGAgYGAgYCAf39/f35+fn5+fn5+fn9+f3+AgICBgYGBgoGCgIGAgYCAgIB/f39/fn5+fn59f31/foB+gH+AgICBgIGAgoCCgIKAgYCAgICAf4B+f35/fn59fn5+fn5/f39/gICBgIGAgYCBgYGBgYGAgYCAf39/f35+fn5+fn5+fn9+gH+Af4CAgIGAgYCBgIGAgYCBgICAf4B/f35/fn59fn1+fn9+f3+AgICBgIGAgoCCgIKAgYCAgH+Af39+f35+fn5+fn5/fn9/f3+AgICAgIGAgYCBgIGAgYCAgYCBf4B/gH9/fn5+fn5+fn5+f39/f4CAgIGAgYGBgYKBgYCBgICAgH9/f35+fn59fn1+fn9+gH+Af4CAgIGAgYCCgIKAgYCBgICAf4B/gH5/fn9+fn5+fn5+f39/gICAgIGAgYGBgYGBgYGBgICAf4B/f35/fn5+fn5+fn5+f3+Af4CAgICAgYCBgIGAgYCBgIGAgICAf39/fn9+fn1+fX5+f35/f4CAgIGAgYGCgYKBgoCBgIGAgIB/f35/fn59fn1+fn9+f39/f4CAgIGAgYCBgIGAgYCBgYCBgIB/gH9/f39+fn5+fn5+fn5+f39/gICAgIGBgYGBgYGBgYGAgICAf39/fn5+fn1+fX5+f35/f4B/gICAgYCBgIKAgoCBgIGAgIB/gH9/fn9+f35+fn5+fn5/f3+AgICAgYCBgIGBgYGBgIGAgIB/gH9/fn9+fn5+fn5+f35/f39/gICAgICAgIGAgYCBgIGAgYCAgICAf39+fn5+fn59fn5+fn9/gH+AgIGBgYKBgoGCgIGAgICAf39/fn9+f31/fn9+f35/f3+AgICAgYCBgIGAgYCBgICAgICAgH+Af39/f39+fn5+fn5+fn5+f3+AgICAgYGBgYGCgYGAgYCBgIB/f39+fn5+fX59f35/fn9/gICAgICBgIGAgYCBgIGAgICAgH+Af4B+f35/fn5+fn5+fn9/f4CAgICBgIGBgYGBgIGAgICAgH9/f39+f35/fn9+f35/f39/f3+AgICAgIGAgYCBgIGAgYCBgICAf39/f35+fn5+fn5+fn9+f3+AgICBgYGBgoGCgIKAgYCAf39/fn9+f31/fX9+f35/f4CAgICAgYCBgIGAgYCBgICAgICAgICAf4B/f39/fn5+fn5+fn5+f39/f4CAgIGBgYGBgYGBgYCBgIB/f39+fn5+fX59f35/fn9/f3+AgICBgIGAgYCBgIGAgYCAgICAf4B/f35/fn9+fn5+fn5/f39/gH+AgIGAgYGBgYGBgYCAgIB/f39/f35/fn9+f35/f39/f39/gH+Af4CAgYCBgIGAgYCAgICAgIB/gH9/fn5+fn5+fn5+fn9/f3+AgIGBgYGBgYGBgYCBgIB/f39+fn5+fn9+f35/f39/gICAgICAgIGAgYCBgICAgICAgICAgIB/gH9/f39/f39+fn5+fn9+f39/gICAgIGBgYGBgYGBgIGAgH9/fn5+fn5+fn5/fn9+f39/f4CAgIGAgYCBgIGAgYCAgICAf4B/gH5/fn9+f35+fn5/fn9/f3+Af4CAgYGBgYGBgYGBgICAgH9/f39/fn9+f35/fn9/f39/f3+Af4CAgICBgIGBgYGBgIGAgICAgH9/fn9+fn5+fn5+fn5+f39/gICAgYGBgYKBgoGBgIGAgH9/f35+fn59f35/fn9/f39/gH+Af4F/gYCBgIGAgICAgICAgIB/gH+Af39/f35/fn5+fn9+f35/f4CAgICBgYGCgYGBgYGAgYCAf39+fn5+fn1+fX9+f35/f4CAgICAgYCBgIGAgYCBgICAgIB/f39/fn9+f35/fn9+f39/f3+Af4B/gICBgYGBgYGBgYGAgICAf39/fn9+f35/fn9+f39/f39/f4B/gICBgIGAgYGBgYGAgICAgH9/f39+f35/fn5+fn5/f39/f4CAgICBgYGBgYGBgYGAgH+Af39+fn5+f31/fn9+gH+Af4CAgIF/gX+BgIGAgYCAgICAgIB/gH+Af4B/f35/fn9/fn9+f35/foB/gH+AgIGBgYGBgYGBgYCAf4B/f35+fn5+fn9+f35/f4B/gICAgYCBgIGAgYCBgIGAgICAgH9/fn9+f35/fn9+f39/f3+Af4B/gH+BgIGAgYGBgYCAgICAgH9/f39+f35/fn9+f35/f39/f4B/gH+BgIGAgYCBgYGBgICAgICAf39/f35/fn9+f35/f39/f4B/gICAgIGBgYGBgYGAgYCAf4B/f35+fn5/fn9+f36Af4B/gIB/gX+Bf4GAgYCBgICAgICAgH+Af4B/gH5/fn9+f39/f35/fn9/gH+AgICAgIGBgYGBgYGBgIB/gH9/fn5+fn5+f35/fn9/gH+AgICAgIGAgYCBgIGAgYCAgIB/f39+f35/fn9+f35/f39/f4B/gH+AgICAgYCBgYGBgYCAgICAf39/f35/fn9+f35/f39/f39/gH+Af4CAgYCBgIGBgIGAgICAf39/f39/fn5+fn5/f39/f39/f4CAgICAgYGBgYGBgYCBgIB/gH9/fn5+fn5+f35/foB/gICAgICBgIF/gYCBgIGAgICAgICAf4B/gH5/fn9+f39/f39/f39/gH+Af4CAgICAgYGBgYGBgYCAgH9/f39+fn5+fn5/fn9+gH+Af4CAgICAgYCBgIGAgYCBgICAf39/f35/fn9+f35/fn9/f39/gICAgICAgYCBgIGAgYCBgICAf39/f35/fn9+f35/fn9/f3+AgICAgICAgYCBgIGAgYCAgICAgIB/f39/f35+fn5+fn9/f39/f4CAgICAgICBgIGBgYCBgIGAgH9/f39+fn5+fn5/fn9+gH+AgICAgIGAgYCBgIGAgYCAgICAf4B/gH5/fn9+f35/fn9/f39/gH+Af4CAgICAgICBgIGAgYCAgICAf39/f35+fn5+fn9+f3+Af4B/gICAgICBgIGAgYCBgIGAgICAf39/fn9+fn5/fn9+f39/f4CAgICAgYCBgIGAgYCBgICAgIB/f39/fn9+f35/fn9/f39/f4CAgICAgICAgICAgICAgICBgICAgH9/f39/fn5+fn5+f39/f39/gICAgICAgICAgYGBgIGAgICAf39/f35+fn5/fn9+f3+Af4CAgICAgYCBgIGAgYCAgICAgIB/gH+Af39+f35/fn9/f39/f3+Af4CAgICAgICAgICAgICAgICAgIB/f39/fn9+fn9+f39/f4B/gICAgICAgICAgYCBgIGAgYCAgIB/f39+f35/fn9+f35/f4CAgICAgYCBgIGAgYCAgICAgICAf39/f39+f35/fn9/f39/f4CAgICAgICAgICAgICAgICAgICAgICAf39/f39/fn5+f39/f39/gICAgICAgICAgICBgIGAgYCAf4B/f39/fn5+fn9+f35/f4B/gICAgICBgIGAgYCBgICAgICAgH9/f39+f35/fn9+f39/f3+AgICAgICAgICAgICAgICAgICAgIB/f39/f39+fn5+f35/f39/gH+AgICAgICAgYCBgIGAgYCAgIB/f39/f35/fn5+f35/f39/gICAgICBgYGAgYCBgIGAgICAf39/f39+f35/fn9+f39/f39/gICAgICAgICAgICAgICAgICAgICAf39/f35/fn9+fn5/f39/f4CAgICAgICBgYCBgICAgICAgIB/f39/f35+fn5+fn5/f39/gICAgICBgYGAgYCBgICAgH+Af4B/f39/f35/fn9+f35/f39/f4CAgICBgICAgICAgICAgICAgICAgH9/f39/fn9+f35/f39/f3+AgICAgICAgICAgICAgICAgIB/gH9/f35/fn9+fn5/fn9/f4CAgICBgYGBgYCBgICAgH+Af39/f39/f35/fn9+f39/f39/gICAgICBgICAgICAgICAgICAf4CAgIB/f39/f39+f35/fn9/f3+AgICAgICBgIGAgICAgICAgIB/gH9/f39/fn9+fn5/fn9/f3+AgICAgIGBgYCAgICAgICAf4B/f39/f39/fn9+f35/f39/f4B/gICBgIGAgYCAgICAgICAgICAgIB/f39/f39/f39/f39/f39/f4CAgICAgIGAgICAgICAgICAgH+Af39/f39+fn5+fn9+f39/gICBgIGBgYGBgYCAgICAf39/f39/f39/f39+f39/f39/f39/gICAgICAgICAgICAgICAgICAgICAf39/f39/f35/fn9+f39/f3+AgICAgYGBgYCBgICAgIB/gH9/f39/f39+f35/fn9/f39/gICAgIGAgYGBgYCAgICAf4B/gH9/f39/f39+f35/fn9+f39/gICAgIGAgYCBgICAgICAgH9/f39/f39/f39/f39/f39/f39/f3+AgICAgIGAgYCBgICAgIB/gH+Af4B/f39+f35/fn9+f35/f4CAgIGAgYGBgYGAgICAf4B/f39/f39/f39+f35/fn9/f39/gICAgICAgYCAgICAgICAgIB/gH+Af4B/f39/f35/fn9+f35/f3+AgICAgYCBgYGBgYCAgIB/gH9/f39/f39/f35/fn9+f39/f4CAgICAgYGBgYGBgICAgIB/gH9/f39/f39/f35/fn9+f39/f4CAgICAgYCBgIGAgICAgIB/f39/f39/f39/f39/fn9+f39/f3+Af4CAgICBgYGBgIGAgICAgH+Af4B/f39/f35/fn9+f35/f39/gICAgYGBgYGBgYGAgIB/gH9/f39/f39+f35/fn9+f39/f3+Af4CAgYCBgIGBgICAgICAgH+Af39/f39/f39/fn9+f35/f39/f4CAgICBgYGBgYGBgICAgH9/f39/f39/f35/fn9+f35/f39/f4CAgYCBgYGBgYGAgICAgH9/f39+f39/f39/fn9+f35/f39/gICAgYCBgIGBgYCBgICAgH9/f39/f39/f39/fn9+f39/f39/f4B/gICBgIGBgYGAgYCAgICAf4B/f39/f39/fn9+f35/fn9/f4CAgICBgYGBgYGBgICAgH+Af39+f35/f35/fn9+f35/f39/f4CAgICBgIGAgYCAgICAgIB/f39/f39/f39/f39+f35/fn9/f39/gICAgIGBgYGBgYCAgICAf4B/f39/f39/fn9+f35/fn9/f4B/gICBgIGBgYGBgYCAgICAf39/f39/fn9/f39+f35/fn9/f3+AgICAgIGBgYGBgICAgH+Af39/f39/f39/fn9+f35/f39/f4B/gH+BgIGAgYGAgYCBgICAgH9/f39/f39/f39/f35/fn9/f39/gICAgYGBgYGBgYGAgICAf4B/f35/fn5/fn9+f35/fn9/f4B/gICBgIGAgYCBgICAgICAgH9/f39/f39/f39/f39/f39/f39/gICAgICAgYGBgYGAgICAgIB/f39/f39/fn9+f35/fn9/f39/gICAgIGAgYGBgYGBgICAgH9/f39/fn9/f39+f35/fn9/f39/gICAgIGAgYGBgYGAgYCAf4B/f39/f39/fn9+f35/fn9/f39/gH+AgIGAgYCBgYGBgICAgH9/f39/f39/f39/f39/fn9+f39/f3+AgICAgYGBgYGBgYCAgIB/gH9/f39/fn9+f35/fn9+f39/gH+AgIGAgYCBgYGBgICAgH9/f39/f39/f39/f39/f39/f39/f4CAgICAgYCBgYGAgYCAgIB/gH9/f39/fn9+f35/fn9+f39/gH+Af4GAgYCBgYGBgIGAgICAf39/f39+f35/f39/f39/f39/f3+AgICAgYCBgYGBgYCAgIB/gH9/f39/f39+f35/fn9+f39/f3+Af4GAgYCBgIGBgIGAgICAf39/f39/f39/f39/f39/f39/f39/gICAgICBgYGBgYGBgICAgH+Af39/f39+f35/fn9+f39/f3+Af4CAgYCBgYGBgYGAgICAf39/f39+f35/f39/f39/f39/f39/gICAgICAgIGBgICAgICAgH+Af39/f39/f35/fn9+f39/f39/f4CAgYCBgIGBgYGAgYCAgH9/f39+f35/fn5/fn9/f39/f39/gICAgICBgYGBgYGBgICAgH+Af39/f39+f35/fn9+f39/f3+Af4CAgYCBgIGBgYGAgYCAgIB/f39/f35/fn9/f39/f39/f39/gH+AgICAgICBgYGBgIGAgH+Af4B/f35/f35/fn9+f35/f39/f4CAgICBgIGBgYGBgYCAgIB/f39/f35/fn5/f39/f39/f39/gICAgICAgICAgIGAgICAgICAf4B/f39/f39/fn9+f35/f39/f4CAgICBgIGAgYGBgYCAgICAf39/f35/fn9+fn9+f39/f39/gH+AgICAgIGAgYCBgICAgICAf4B/f39/f35/fn9+f35/f39/f4CAgICBgIGAgYCAgYCAgICAf39/f39/f39/f39/f39/f39/f3+AgICAgICAgICBgYGAgICAf4B/gH9/f39/fn9+f35/fn9/f39/gICAgIGAgYGBgYCAgICAf39/f39/fn9/f39/f39/f39/gH+AgICAgICAgICAgICAgICAgIB/gH9/f39/f39+f35/f39/f39/gH+AgIGAgYCBgYGBgICAgIB/f39/f39+f35+f39/f39/f3+AgICAgICAgYGBgYGAgICAgIB/f39/f39/fn9+f35/f39/f39/gH+AgICAgYCAgICBgICAgH+Af39/f39/f39/f39/f39/f39/f3+AgICAgICAgYGBgICAgICAgH9/f39/fn9+f35/fn9/f39/gH+AgIGAgYCBgYCBgIGAgH+Af39/f39+f35/f39/f39/f3+Af4CAgICAgICAgICAgICAgICAgH9/f39/f39/f35/fn9/f39/f3+Af4CAgYCBgICBgIGAgYCAgH9/f39/f35/fn9+f39/f39/f4B/gICAgICBgYGBgYCAgICAgH9/f39/f39+f35/fn9/f39/f3+AgICAgYCBgICAgICAgICAf4B/f39/f39/f39/f39/f39/f39/gICAgICAgYGBgYGAgICAgH+Af39/f39+f35/fn9+f39/f3+AgICAgYCBgYGBgIGAgICAf39/f39/f39/f39/f39/f39/f39/gICAgICAgICAgICAgICAgH9/f39/f39/f35/fn9/f39/f3+Af4CAgICBgICBgIGAgYCAf4B/f39/f39/fn9+f39/f39/f39/gICAgICAgYGBgIGAgICAgH9/f39/f39/f35/fn9/f39/f3+AgICAgYCBgICAgICAgH+Af4B/f39/f39/f39/f39/f39/f39/f3+AgICAgICBgIGAgYCAgICAf39/f39/f39/fn9+f39/f3+Af4CAgYCBgIGBgIGAgICAf4B/f39/f39/f39/f39/f39/f39/f4CAgICAgICAgICAgICAgICAf39/f39/f39/fn9+f39/f3+Af4B/gICBgIGAgICAgYCAf4B/gH9/f39/f39/f39/f39/f39/f4CAgICAgICBgIGAgICAgICAf39/f39/f39/fn9+f39/f3+Af4CAgICBgIGAgICAgICAf4B/f39/f39/f39/f39/f39/f39/f39/gH+AgICAgICAgICAgICAgIB/f39/f39/f39+f35/f39/f4B/gICBgIGAgYCAgYCAgIB/gH9/f39/f39/f39/f39/f39/f4B/gICAgICAgICAgICAgICAgH9/f39/f39/f39/f39/f39/f4B/gH+AgIGAgICAgICAf4B/gH+Af39/f39/f39/f39/f39/f39/f3+AgICAgIGAgYCAgICAgIB/f39/f39/fn9+f35/f39/f4B/gICBgIGAgYCAgICAf4B/gH9/f39/f39/f39/f39/f39/f39/gH+Af4CAgICAgYCBgICAgICAf39/f39/f39/f39/f39/f39/gH+AgIGAgYCAgICAgIB/gH9/f39/f39/f39/f39/f39/f39/gH+Af4CAgICAgICAgICAgICAf39/f39/f39/f35/f39/f39/gH+Af4GAgYCAgICAgIB/gH+Af4B/f39/f39/f39/f39/f39/f3+Af4CAgICAgYCBgIGAgICAgH9/f39/fn9+f35/fn9/f39/gH+AgIGAgYCBgICAgIB/gH+Af39/f39/f39/f39/f39/f39/gH+Af4B/gICAgICBgIGAgYCAgIB/f39/f39+f35/fn9/f39/gH+Af4GAgYCBgYGBgIGAgH+Af39/f39/f39/f39/f39/f39/f3+Af4B/gICAgICAgICAgICAgIB/f39/f39+f35/f39/f39/gH+Af4F/gYCBgICBgIGAgH+Af4B/f39/f39/f39/f39/f39/f39/f4B/gICAgICBgIGAgICAgIB/f39/f39+f35/fn9/f39/gH+Af4GAgYCBgIGAgICAgH+Af4B/f39/f39/f39/f3+Af4B/gH+Af4B/gH+AgICAgIGAgYCAgICAgH9/f39/f39/f39/f39/f3+Af4CAgYCBgIGBgIGAgH+Af4B/f39/f39/f39/f39/f39/gH+Af4B/gICAgICAgICAgICAgICAf39/f39/f35/fn9/f39/f3+Af4B/gX+BgIGAgICAgX+Af4B/gH9/f39/f39/f39/f39/f3+Af4B/gICAgICAgIGAgYCAgICAf4B/f39/f35/fn9+f39/f3+Af4B/gH+BgIGAgYCAgICAf4B/gH9/f39/f39/f39/f4B/gH+Af4B/gH+Af4CAgIGAgYCBgICAgIB/f39/f39/f39+f39/f39/f4B/gH+BgIGAgYCAgICAgIB/gH9/f39/f39/f39/f39/f3+Af4B/gH+Af4CAgICAgICAgICAgH9/f39/f39/fn9+f39/f39/f4B/gH+Af4GAgYCAgICAgIB/gH+Af39/f39/f39/f39/f39/f39/f3+Af4CAgICAgYCBgICAgIB/f39/f39/fn9+f35/f39/f4B/gH+Bf4F/gYCBgICAgIB/gH+Af39/f39/f39/f39/gH+Af4B/gH+Af4B/gICAgICBgIGAgICAgH9/f39/f39/f39/f39/f39/gH+Af4B/gYCBgICAgICAgH+Af39/f39/f39/f39/f39/f39/gH+Af4B/gICAgICAgICAgICAf39/f39/f39+f39/f39/f4B/gH+Af4F/gYCAgICAgIB/gH+Af4B/f39/f39/f39/f3+Af39/f39/f4B/gICAgICBgIGAgICAgIB/f39/f39+f35/fn9/f39/gH+Af4F/gX+BgIGAgICAgH+Af4B+gH6Af39/f39/gH+Af4B/gH+Af4B/gH+AgICAgIGAgYCAgICAf39/f39/f39/f39/f39/f3+Af4B/gYCBgIGAgICAgICAf4B/gH9/f39/f39/f39/f4B/gH+Af4B/gH+AgICAgICAgICAgIB/gH9/f39/f39/f39/f39/gH+Af4B/gH+AgICAgICAgH+Af4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gH+AgICAgICAgYCAgICAgH9/f39/f35/f39/f39/f3+Af4B/gX+BgIGAgICAgICAf4B/gH5/fn9/f39/f3+Af4B/gH+Af4B/gH+AgICAgICAgYCAgICAgH9/f39/f39/f39/f39/f3+Af4B/gH+AgICAgICAgICAgIB/gH9/fn9+f39/f39/f4B/gH+Af4B/gH+AgICAgICAgICAgICAgH9/f39/f39/f39/f39/f3+Af4B/gH+Af4CAgICAgICAf4B/gH+Af39/f39/f39/f4B/gH+Af4B/gH+AgICAgICAgICAgICAgIB/f39/f39/f39/f39/f39/f4B/gH+Af4GAgYCAgICAf4B/gH+Afn9+f39/f3+Af4B/gH+Af4B/gH+Af4CAgICAgICAgICAgICAf39/f39/f39/f39/f4B/f39/gH+Af4B/gYCBgICAgIB/gH+Af4B+f35/f39/f39/gH+Af4B/gH+Af4CAgICAgICAgICAgH+Af39/f39/f39/f39/f4B/gIB/gH+Af4B/gH+AgICAgICAgH+Af4B/f39/f39/f39/gH+Af4B/gH+Af4B/gICAgICAgICAgICAf39/f39/f39/f39/f39/f4B/gH+Af4B/gH+BgICAgICAgH+Af4B+f35/f39/f39/gH+Af4B/gH+Af4B/gH+AgICAgICAgICAgIB/f39/f39/f39/f39/f39/f3+Af4B/gH+AgICAgICAgICAf4B/gH5/fn9/f39/f3+Af4B/gH+Af4B/gICAgICAgICAgICAf4B/f39/f39/f39/f39/gICAgH+Af4B/gH+Af4CAgICAgICAf4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gH+AgICAgICAgICAgIB/gH9/f39/f39/f39/f39/gH+Af4B/gH+Af4CAgICAgICAf4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gH+Af4CAgICAgICAgIB/gH9/f39/f39/f39/f39/f3+Af4B/gH+Af4CAgICAgICAf4B/gH9/fn9+f39/f3+Af4B/gICAgICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/gH+AgICAgIB/gH+Af4B/gICAgICAf4B/gH+Af39/f39/f39/f4B/gH+Af4B/gICAgICAgICAgICAgIB/gH9/f39/f39/f39/f39/f4CAgICAgICAf4B/gICAgICAf4B/gH+Afn9+f39/f39/f4B/gH+AgICAgICAgIB/gICAgICAgIB/gH+Af39/f39/f39/f39/f39/gICAgICAf4B/gICAgICAgIB/gH+Afn9+f35/f39/f4B/gH+AgICAgICAgICAgICAgICAgIB/gH9/f39/f39/f39/f3+Af4CAgICAgICAf4B/gH+Af4CAgIB/gH+Af4B/f39/f39/f39/gH+Af4CAgICAgICAgICAgICAgIB/gH+Af39/f39/f39/f39/f3+AgICAgICAgICAgICAgICAgIB/gH+Af4B/f35/f39/f39/gH+AgICAgICAgICAgICAgICAgIB/gH+Af4B/f39/f39/f39/f39/gH+AgICAf4B/gH+AgICAgICAgH+Af4B/f35/f39/f39/gH+Af4CAgICAgICAgICAgICAgIB/gH9/f39/f39/f39/f39/f4B/gH+AgICAgICAgH+Af4B/gICAgICAf4B/gH9/f39/f39/f3+Af4B/gICAgICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/f3+Af4CAgICAgICAgICAgICAgICAf4B/f39/f39/f39/f3+AgICAgICAgICAgICAgICAgH+AgH+Af4B/f39/f39/f39/f39/f3+Af4B/gICAgICAgICAgICAgICAgIB/gH9/f39/f39/f39/f4CAgICAgICAgICAgICAgICAf39/f39/f39/f39/f39/f39/gH+Af4CAgICAgICAgIB/gH+Af4B/gH9/f39/f39/f39/f4B/gICAgICAgICAgICAf4B/gH+Af4B/f4B/gH9/f39/f39/f39/f3+Af4B/gICAgICAgICAgH+Af4B/gH9/f39/f39/f39/f39/gICAgICAgICAgICAgIB/gH+Af4B/f39/f39/f39/f39/f4B/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH9/f39/f39/f39/f39/f3+AgICAgICAgICAgICAgH+Af39/f39/f39/f39/f39/f39/f3+Af4B/gH+AgICAgICAgH+Af4B/gH+Af39/f39/f39/f39/gH+AgICAgICAgICAgICAgH+Af4B/f39/f39/f39/f39/f39/f4CAgIB/gH+AgICAgICAgICAf4B/gH9/f39/f39/f39/f39/f3+AgICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/f3+AgICAgIB/gH+Af4CAgICAgICAf4B/gH+Af39/f39/f39/f39/f39/gICAgICAgICAgICAgICAgIB/f39/f39/f39/f39/f3+Af4B/gICAgH+Af4B/gICAgICAf4B/gH+Af39/f39/f39/f39/f3+Af4CAgICAgICAgICAgICAgIB/gH9/f39/f39/f39/f39/f39/gH9/gH+Af4B/gICAgICAgICAgH+Af39/f39/f39/f39/f39/f4CAgICAgICAgICAgICAgIB/f39/f39/f39/f39/f3+Af4B/gH9/gH+Af4B/gH+AgICAgICAgH+Af4B/gH9/f39/f39/f39/f39/gICAgICAgICAgICAgICAgH9/f39/f39/f39/f3+Af4B/gH+AgH+Af4B/gH+Af4CAgICAgH+Af4B/gH9/f39/f39/f39/f4B/gH+AgICAgICAgICAgICAgIB/f39/f39/f39/f3+Af4B/gH+Af3+Af4B/gH+Af4CAgICAgICAf4B/gH9/f39/f39/f39/f4B/gICAgICAgICAgICAgICAgH9/f39/f39/f39/f3+Af4B/gH+AgICAf4B/gH+Af4B/gICAgICAf4B/gH+Af39/f39/f39/f39/gH+AgICAgICAgICAgICAgIB/f39/f39/f39/f3+Af4B/gH+AgICAf4B/gH+Af4B/gICAgH+Af4B/gH+Af39/f39/f39/f4B/gH+AgICAgICAgICAgICAgICAf39/f39/f39/f39/f4B/gH+Af4B/f4B/gH+Af4B/gICAgICAf4B/gH+Af39/f39/f39/f4B/gH+AgICAgICAgICAgICAgH+Af39/f39/f39/f39/f4B/gH+Af4CAgIB/gH+Af4B/gH+AgICAgIB/gH+Af39/f39/f39/f39/f3+Af4CAgICAgICAgICAgICAf39/f39/f39/f39/f4B/gH+Af4CAgIB/gH+Af4B/gH+AgICAf4B/gH+Af39/f39/f39/f39/gICAgICAgICAgICAgICAgH9/f39/f39/f39/f39/f4B/gH+Af4B/gICAgH+Af4B/gH+Af4CAgIB/gH+Af4B/gH9/f39/f39/f4CAgICAgICAgICAgICAgICAf39/f39/f39/f39/f4B/gH+Af4B/gICAgH+Af4B/gH+Af4B/gIB/gH+Af4B/gH+Af4B/gH+Af4CAgICAgICAf4B/gICAgICAgH9/f39/f39/f39/f4B/gH+Af4B/gICAgH+Af4B/gH+Af4B/gIB/gH+Af4B/f39/f39/f39/f4CAgICAgICAgICAgICAgICAf39/f39/f39/f39/f4B/gH+Af4B/gH+AgH+Af4B/gH+Af4B/gICAgH+Af4B/gH9/f39/f39/f39/gICAgICAgICAgICAgICAgICAf39/f39/f39/f39/gH+Af4B/gH+AgICAf4B/gH+Af4B/gH+Af4CAf4B/gH+Af4B/gH+Af4B/gH+AgICAf4B/gICAgICAgICAf4B/gH9/f39/f39/gH+Af4B/gH+Af4CAf4B/gH+Af4B/gH+AgH+Af4B/gH9/f39/f39/f4B/gICAgICAgICAgICAgICAgH+Af39/f39/f39/f39/gH+Af4B/gH+Af4CAf4B/gH+Af4B/gH+Af4CAf4B/gH+Af4B/gH+Af4B/gH+AgICAgICAgICAgICAgICAf39/f39/f39/f39/f3+Af4B/gH+Af4CAgIB/gH+Af4B/gH+Af4B/f39/gH+Af4B/gH+Af4B/gH+Af4CAgIB/gH+Af4B/gH+Af4B/f39/f39/f39/gH+Af4B/gH+Af4CAgIB/gH+Af4F/gH+Af4B/f39/f39/f39/gH+Af4B/gH+AgICAgICAgICAgIB/gH+Af39/f39/f39/f39/gH+Af4B/gH+Af4B/gIB/gH+Af4B/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gH+Af4CAgICAgICAgICAgH+Af39/f39/f39/f39/f3+Af4B/gH+Af4B/gICAgH+Af4B/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+Af4CAgICAgICAgIB/gH+Af4B/f39/f39/f39/f3+Af4B/gH+Af4B/gICAgICAf4F/gX+Af4B/gH9/f39/f39/f39/f4B/gH+Af4CAgICAgICAgICAgH+Af4B/f39/f39/f39/f3+Af4B/gH+Af4B/gH+AgH+Af4F/gX+Af4B/gH9/f39/f4B/gH+Af4B/gH+Af4B/gICAgICAgIGAgICAf4B/f39/f39/f39/f39/f4B/gH+Af4B/gH+AgICAf4B/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+Af4B/gICAgICAgIB/gH+Af4B/gH9/f39/f39/f39/f4B/gH+Af4B/gH+AgICAf4B/gX+Bf4B/gH+Af39/f39/f39/f39/gH+Af4B/gICAgICAgICAgICAf4B/gH9/f39/f39/f39/f4B/gH+Af4B/gH+Af4CAgIB/gX+Bf4B/gH+Af39/f39/f39/f39/gH+Af4B/gH+AgICAgICAgYCAgIB/gH9/f39/f39/f39/f39/gH+Af4B/gH+Af4CAgIB/gH+Af4B/gH+Af39/f39/f39/f39/gH+Af4B/gH+Af4CAgICAgICAgIB/gH+Af4B/f39/f39/f39/f3+Af4B/gH+Af4CAgICAgX+Bf4F/gH+Af39/f39/f39/f39/f3+Af4B/gH+AgICAgICAgICAgIB/gH+Af39/f39/f39/f39/gH+Af4B/gH+Af4B/gICAgH+Bf4F/gH+Af4B/f39/f39/f39/f39/f4B/gH+Af4CAgICAgICBgIGAgH+Af4B/f39/f39/f39/f3+Af4B/gH+Af4B/gICAgICAf4F/gH+Af4B/f39/f39/f39/gH+Af4B/gH+Af4B/gICAgICAgIB/gH+Af4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gICAgICBf4F/gX+Af4B/f39/f39/f39/f3+Af4B/gH+Af4CAgICAgICAgIB/gH+Af4B/f39/f39/f39/f3+Af4B/gH+Af4B/gH+AgICAf4F/gX+Af4B/gH9/f39/f39/f39/f4B/gH+Af4B/gICAgICAgIGAgH+Af4B/gH9/f39/f39/f39/f4B/gH+Af4B/gICAgICAf4B/gH+Af4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gICAgICAgICAgH+Af4B/gH9/f39/f39/f39/f4B/gH+Af4B/gH+AgICAgIF/gX+Bf4B/gH9/f39/f39/f39/f39/gH+Af4B/gICAgICAgICAgICAf4B/gH9/f39/f39/f39/f4B/gH+Af4B/gH+AgICAgIB/gX+Bf4B/gH+Af39/f4B/gH+Af4B/gH+Af4B/gH+AgICAgICAgICAgIB/gH+Af39/f39/f39/f39/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH+Af39/f39/gH+Af4B/gH+Af4B/gH+Af4CAgICAgICAf4B/gH+Af4B/f39/f39/f4B/gH+Af4B/gH+Af4CAgIB/gX+Bf4F/gH+Af39/f39/f39/f4B/gH+Af4B/gH+AgICAgICAgICAgIB/gH+Af39/f39/f39/f39/gH+Af4B/gH+Af4B/gIB/gH+Af4F/gH+Af4B/f39/gH+Af4B/gH+Af4B/gH+Af4CAgICAgICAgICAgH+Af39/f39/f39/f39/gH+Af4B/gH+Af4CAgICAgH+Af4B/gH+Af39/f39/f3+Af4B/gH+Af4B/gH+Af4B/gICAgICAgIB/gH+Af4B/f39/f39/f39/gH+Af4B/gH+Af4B/gICAgICBf4F/gH+Af4B/f39/f39/f39/gH+Af4B/gH+Af4CAgICAgICAgICAgH+Af4B/f39/f39/f39/f3+Af4B/gH+Af4B/gICAgICAf4B/gH+Af4B/gH9/f39/f4B/gH+Af4B/gH+Af4B/gICAgICAgICAgH+Af4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gICAgICAgIB/gH+Af4B/gH9/f39/f4B/gH+Af4B/gH+Af4B/gH+AgICAgICAgH+Af4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gICAgICAgIB/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+Af4B/gICAgICAgICAgH+Af4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH9/f39/f4B/gH+Af4B/gH+Af4B/gICAgICAgICAgICAf4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+Af4B/gH+AgICAgICAgICAf4B/gH+Af39/f39/f3+Af4B/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH+Af39/f39/gH+Af4B/gH+Af4CAgICAgICAgICAgICAgIB/gH+Af39/f39/f39/f4B/gH+Af4B/gH+Af4CAgIB/gH+Af4B/gH+Af39/f39/gH+Af4B/gH+Af4B/gICAgICAgICAgICAgIB/gH+Af39/f39/f39/f4B/gH+Af4B/gH+Af4CAgICAgH+Af4B/gH+Af39/f39/f3+Af4B/gH+Af4B/gH+Af4CAgICAgICAgIB/gH+Af4B/f39/f39/f4B/gH+Af4B/gH+Af4CAgICAgH+Af4B/gH+Af4B/f39/f3+Af4B/gH+Af4B/gICAgICAgICAgICAgIB/gH+Af4B/f39/f39/f39/gH+Af4B/gH+Af4B/gICAgH+Af4B/gH+Af4B/f39/gH+Af4B/gH+Af4B/f39/f3+AgICAgICAgICAgH+Af4B/f39/f39/f39/gH+Af4B/gH+Af4CAgICAgICAgIB/gH+Af4B/f39/f39/f4B/gH+Af4CAgICAgICAgICAgICAgIB/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+Af4B/gICAgICAgIB/gH+Af4B/f39/f39/f4B/gH+Af4B/gIB/gICAgICAgICAgICAgH+Af4B/f39/f39/f39/gH+Af4B/gH+Af4B/gH+AgICAf4B/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gH+Af39/gICAgICAgICAgH+Af4B/gH9/f39/f39/f3+Af4B/gH+Af4B/gICAgICAgIB/gH+Af4B/gH9/f39/f4B/gH+Af4B/gICAgICAgICAgICAf4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+AgICAgICAgICAgICAgH+Af4B/gH9/f39/f39/gH+Af4B/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+Af4CAgICAgICAgICAgH+Af4B/gH+Af39/f39/f3+Af4B/gH+Af4B/gH+AgICAgICAgH+Af4B/gH+Af39/f39/gH+Af4B/gH+AgICAgICAgICAgIB/gH+Af4B/gH+Af4B/gIB/gH+Af4B/gH+Af39/f39/f4B/gICAgICAf4B/gH+Af4B/f39/gH+Af4B/gH+Af4CAgICAgICAgICAgH+Af4B/gH+Af39/f39/gH+Af4B/gH+Af4B/gH+Af4B/gICAgH+Af4B/gH+Af4B/f4B/gH+Af4B/gH+Af39/f4B/gH+AgICAgICAf4B/gH+Af4B/f4B/gH+Af4B/gH9/f39/f3+Af4CAgICAgICAf4B/gH+Af4B/f39/gH+Af4B/gH+Af4CAgICAgICAgIB/gH+Af4B/gH+Af4B/gIB/gH+Af4B/gH+Af39/f39/f39/gICAgICAf4B/gH+Af4B/f39/gH+Af4B/gH+Af39/f4CAgICAgICAgICAf4B/gH9/f39/f39/gH+Af4B/gH+Af4B/f39/f39/gH+AgICAf4B/gH+Af4B/gIB/gH+Af4B/gH+Af39/f39/gH+AgICAgICAf4B/gH+Af4B/f4B/gH+Af4B/gH+Af39/f39/f4B/gICAgICAgIB/gH+Af4B/gH9/gH+Af4B/gH+Af4B/gICAgICAgICAgH+Af4B/gH+Af4B/f4B/gH+Af4B/gH+Af39/f39/f39/f3+AgICAgIB/gH+Af4B/gH9/gH+Af4B/gH+Af4B/gH+AgICAgICAgICAf4B/gH+Af39/f39/gH+Af4B/gH+Af4CAf39/f39/f3+Af4CAf4B/gH+Af4B/gH+AgH+Af4B/gH+Af4B/f39/f3+AgICAgICAf4B/gH+Af4B/f39/gH+Af4B/gH+Af4B/f39/f39/f3+AgICAgIB/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gH+AgICAgICAgICAf4B/gH+Af39/f39/gH+Af4B/gH+Af4B/f39/f39/f39/f4CAgICAgH+Af4B/gH+Af3+Af4B/gH+Af4B/gH+Af4CAgICAgICAgIB/gH+Af4B/f39/f3+Af4B/gH+Af4B/f39/f39/f39/f4CAgIB/gH+Af4B/gH+Af4CAf4B/gH+Af4B/gH9/f39/f4CAgICAgIB/gH+Af4B/gH9/f3+Af4B/gH+Af4B/f39/f39/f39/gICAgICAgICAf4B/gH+Af39/f4B/gH+Af4B/gH+Af4CAgICAgICAgIB/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gH9/f39/f39/f39/gICAgICAf4B/gH+Af4B/f4B/gH+Af4B/gH+Af39/f4CAgICAgIB/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gIB/gH+Af39/f39/gH+AgICAf4B/gH+Af4B/gIB/gH+Af4B/gH+Af4B/f39/gH+Af4B/gH+Af4B/gH+Af3+Af4B/gH+Af4B/gH9/f39/f39/gH+Af4CAgICAgIB/gH+Af4B/f4B/gH+Af4B/gH+Af3+Af4B/gH+Af4B/gH+Af4B/gH9/f39/f4B/gH+Af4B/gICAgH+Af39/f39/f39/gICAf4B/gH+Af4B/gIB/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gICAgH+Af4B/gH+Af4B/gICAgIB/gH+Af4B/gH9/gH+Af4B/gH+Af4B/f39/f3+Af4B/gH+Af4B/gH+Af4B/f4B/gH+Af4B/gH+Af3+Af4B/gH+Af4B/gICAgIB/gH+Af4B/f39/gH+Af4B/gH+Af4B/f4B/gH+Af4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+AgICAf4B/gH9/f39/f3+Af4B/gH+Af4B/gH+AgH+Af4B/gH+Af4B/f39/f3+Af4B/gH+Af4B/gH+Af4B/f39/gH+Af4B/gH+AgICAf4B/gH+Af4B/gH+Af4B/gH+Af4B/gH+AgH+Af4B/gH+Af4B/gH9/f39/f4B/gH+Af4B/gH+Af4B/f39/gH+Af4B/gH+Af4CAf4B/gH+Af4B/gH+Af4B/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gH+AgH+Af4B/gH+Af4B/gH+Af4B/f39/gH+Af4B/gH+Af4CAgIB/gH+Af4B/gH+Af4B/gH+Af4B/gH+AgH+Af4B/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gIB/gH+Af4B/gH+Af4B/gH+Af4B/f39/f3+Af4B/gH+Af4B/gH+Af39/f39/gH+Af4B/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gIB/gH+Af4B/gH+Af4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+Af4B/f4B/gH+Af4B/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gICAgH+Af4B/gH+Af4B/gH9/f4B/gH+Af4CAgICAgH+Af4B/gH+Af4B/f39/f3+Af4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+AgH+Af4B/gH+Af4B/gH9/f39/f39/f39/f4B/gH+Af4B/gH+Af4B/f39/f3+Af4B/gH+Af4B/gH+Af3+Af4B/gH+Af4B/gH+AgH+Af4B/gH+Af4B/gH+Af4B/gH+Af4B/f39/gH+Af4B/gH+Af4B/gH9/gH+Af4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gICAgICAf4B/gH+Af4B/f39/f39/f39/f4B/gH9/gH+Af4B/gH+Af4B/f39/f39/f4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+AgICAf4B/gH+Af4B/gH9/f39/f39/f39/f39/gH+Af4B/gICAgICAgIB/gH9/f4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+AgICAf4B/gH+Af4B/gH9/f39/f39/f39/f39/gH+Af4B/gH+AgICAgICAgH+Af4B/gH+Af4B/gH+Af39/f39/gH+Af4B/gH+AgICAgIB/gH+Af4B/f39/f39/f39/f39/f39/f3+Af4B/gH+AgICAgIB/gH+Af4B/gH+Af4B/gH+Af4B/f39/f3+Af4B/gH+Af4CAgICAgH+Af4B/gH9/f39/f39/f39/f39/f3+Af4B/gH+Af4CAgICAgH+Af4B/gH+Af4B/gH+Af4B/f39/f3+Af4B/gH+Af4CAgICAgH+Af4B/gH9/f39/f39/f39/f39/f3+Af4B/gH+Af4B/gICAgICAf4B/gH+Af4B/gH+Af4B/f39/f3+Af4B/gH+Af4CAgICAgH+Af4B/gH+Af39/f39/f39/f39/f3+Af4B/gH+Af4B/gH+AgH+Af4B/gH+Af4B/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gICAgICAf4B/gH+Af4B/f39/f39/f39/f39/f4B/gH+Af4CAgICAgICAf4B/gH+Af4B/gH+Af4B/gH9/f3+Af4B/gH+Af4B/gICAgH+Af4B/gH+Af4B/gH9/f39/f39/f39/f4B/gH+Af4B/gH+AgICAf4B/gH+Af4B/gH+Af4B/gH9/f39/f4B/gH+Af4B/gICAgICAf4B/gH+Af4B/f39/f39/f39/f39/f4B/gH+Af4B/gH+Af4B/f4B/gH+Af4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH9/f39/f39/f39/f39/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH+Af4B/gH+Af39/f4B/gH+Af4B/gH+AgICAgIB/gH+Af4B/f39/f39/f3+Af4B/gH9/gH+Af4B/gH+Af4B/gIB/gH+Af4B/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+AgICAgIB/gH+Af4B/f39/f39/f39/f39/gH+AgICAgIB/gH+Af4B/gH9/gH+Af4B/gH+Af4B/gH+Af39/f39/gH+Af4B/gH+Af4CAgICAgH+Af4B/gH9/f39/f39/f39/f39/gH+Af4B/gH+AgICAgIB/gH+Af4B/gH+Af4B/f39/f39/f39/gH+Af4B/gH+AgICAgICAgH+Af4B/gH9/f39/f39/f39/f3+Af4CAf4B/gH+Af4B/gICAgH+Af4B/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+AgICAgICAgH+Af4B/gH9/f39/f39/f39/f39/f4CAgICAgICAf4B/gH+AgH+Af4B/gH+Af4B/gH+Af39/f39/f3+Af4B/gH+Af4CAgICAgICAf4B/gH+Af39/f39/f39/f39/f3+AgIB/gH+Af4B/gICAgH+Af4B/gH+Af4B/gH9/f39/f39/f3+Af4B/gH+Af4CAgICAgICAf4B/gH+Af39/f39/f39/f3+Af4B/gICAgH+Af4B/gH+Af4CAf4B/gH+Af4B/gH+Af39/f39/f3+Af4B/gH+Af4CAgICAgICAf4B/gH+Af39/f39/f39/f39/f4B/gICAgICAgIB/gH+Af4B/f4B/gH+Af4B/gH+Af4B/f39/f39/f4B/gH+Af4B/gICAgICAgIB/gH+Af4B/f39/f39/f39/f39/gICAgICAf4B/gH+Af4CAf4B/gH+Af4B/gH+Af39/f39/f3+Af4B/gH+Af4B/gICAgICAgIB/gH+Af4B/f39/f39/f39/f39/gICAgICAf4B/gH+Af4B/gIB/gH+Af4B/gH+Af39/f39/f3+Af4B/gH+Af4B/gICAgICAgIB/gH+Af4B/f39/f39/f39/f39/gICAgICAgICAgH+Af4B/gH9/gH+Af4B/gH+Af4B/f39/f39/f4B/gH+Af4B/gH+AgICAgICAgH+Af4B/gH9/f39/f39/f39/f4CAgICAgICAgH+Af4B/gH9/gH+Af4B/gH+Af4B/f39/f39/f4B/gH+Af4B/gICAgICAgICAgH+Af4B/gH9/f39/f39/f39/gH+AgICAgIB/gH+Af4B/gH+AgH+Af4B/gH+Af4B/f39/f39/f39/gH+Af4CAgICAgICAgICAgICAf4B/gH9/f39/f39/f39/gH+AgICAgICAgH+Af4B/gH+Af3+Af4B/gH+Af4B/gH9/f39/f39/gH+Af4B/gH+AgICAgICAgICAf4B/gH9/f39/f39/f39/f3+AgICAgICAgICAf4B/gH+Af3+Af4B/gH+Af4B/gH9/f39/f39/gH+Af4B/gICAgICAgICAgICAgIB/gH+Af39/f39/f39/f39/f4CAgICAgICAf4B/gH+Af4CAf4B/gH+Af4B/gH9/f39/f39/gH+Af4B/gICAgICAgICAgICAgIB/gH+Af39/f39/f39/f39/f4CAgICAgICAf4B/gH+Af4B/f4B/gH+Af4B/gH+Af39/f39/f3+Af4B/gH+Af4CAgICAgICAgICAgH+Af4B/f39/f39/f39/f3+AgICAgICAgIB/gH+Af4B/f4B/gH+Af4B/gH+Af39/f39/f3+Af4B/gH+AgICAgICAgICAgICAgH+Af4B/f39/f39/f39/f39/gICAgICAgIB/gH+Af4B/gIB/gH+Af4B/gH+Af39/f39/f39/f4B/gH+AgICAgICAgICAgICAgH+Af4B/f39/f39/f39/f39/gICAgICAgICAgH+Af4B/f39/gH+Af4B/gH+Af4B/f39/f39/f4B/gH+Af4CAgICAgICAgICAgICAf4B/f39/f39/f39/f39/f4CAgICAgICAgH+Af4B/gH9/gH+Af4B/gH+Af4B/f39/f39/f4B/gH+AgICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/gICAgICAgICAgH+Af4B/gH9/gH+Af4B/gH+Af4B/f39/f39/f4B/gH+Af4CAgICAgICAgICAgICAf4B/gH9/f39/f39/f39/f4CAgICAgICAgICAf4B/gH9/f3+Af4B/gH+Af4B/gH9/f39/f4B/gH+Af4B/gICAgICAgICAgICAgIB/gH9/f39/f39/f39/f3+AgICAgICAgICAf4B/gH+Af3+Af4B/gH+Af4B/gH9/f39/f4B/gH+Af4CAgICAgICAgICAgICAgIB/gH+Af39/f39/f39/f39/gICAgICAgICAf4B/gH+Af39/f4B/gH+Af4B/gH9/f39/f39/f3+Af4CAgICAgICAgICAgICAgIB/gH+Af39/f39/f39/f39/gICAgICAgICAgIB/gH+Af4B/f4B/gH+Af4B/gH9/f39/f39/f3+Af4B/gICAgICAgICAgICAgICAgH+Af39/f39/f39/f39/f3+AgICAgICAgICAgH+Af4B/f4B/gH+Af4B/gH+Af39/f39/f3+Af4B/gICAgICAgICAgICAgICAgH+Af4B/f39/f39/f39/f3+AgICAgICAgICAgH+Af4B/f4B/gH+Af4B/gH+Af39/f39/f39/f4B/gH+AgICAgICAgICAgICAgH+Af4B/f39/f39/f39/f3+AgICAgICAgICAgICAf4B/gH9/gH+Af4B/gH+Af4B/f39/f3+Af4B/gH+AgICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/f4CAgICAgICAgICAf4B/gH+AgH+Af4B/gH+Af4B/f39/f39/f4B/gH+AgICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/f4CAgICAgICAgICAf4B/gH9/gH+Af4B/gH+Af4B/f39/f39/f39/gH+AgICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/f3+AgICAgICAgICAgIB/gH+Af3+Af4B/gH+Af4B/f39/f39/f39/gH+AgICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/f3+AgICAgICAgICAgICAgH+Af3+Af4B/gH+Af4B/f39/f39/f39/gH+AgICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/f39/gICAgICAgICAgICAgH+Af4CAf4B/gH+Af4B/f39/f39/f39/f39/f4CAgICAgICAgICAgICAgIB/f39/f39/f39/f39/f39/gICAgICAgICAgICAgICAgH+Af4B/gH+Af4B/f39/f39/f39/f39/f4CAgICAgICAgICAgICAgIB/gH9/f39/f39/f39/f39/f4CAgICAgICAgICAgICAgICAf4B/gH+Af4B/f39/f39/f39/f3+AgICAgICAgICAgICAgICAgIB/gH9/f39/f39/f39/f39/f4CAgICAgICAgICAgICAgICAgIB/gH+Af4B/f39/f39/f39/f39/f3+AgICAgICAgICAgICAgICAf39/f39/f39/f39/f39/f4CAgICAgICAgICAgICAgICAf4B/gH+Af4B/f39/f39/f39/f39/f3+Af4CAgICAgICAgICAgICAgH9/f39/f39/f39/f39/f4B/gH+AgICAgICAgICAgICAgIB/gH+Af4B/gH9/f39/f39/f39/f4CAgICAgICAgICAgICAgICAgH+Af39/f39/f39/f39/f4B/gICAgICAgICAgICAgICAgICAgH+Af4B/gH+Af39/f39/f39/f39/f4CAgICAgICAgICAgICAgICAf4B/f39/f39/f39/f4B/gICAgICAgICAgICAgICAgIB/gH+Af4B/gH+Af4CAf4B/gH9/f39/f4B/gICAgICAgICAgICAgICAgIB/gH9/f39/f39/f39/f3+Af4CAgICAgICAgICAgICAgH+Af4B/gH+Af39/f39/f3+Af4CAgICAgICAgICAgICAgICAgICAf4B/gH9/f39/f39/f39/gH+AgICAgICAgICAgICAgICAgICAf4B/gH+Af4B/f39/f39/f39/f4B/gICAgICAgICAgICAgICAf4B/f39/f39/f39/f39/gICAgICAgICAgICAgICAgICAgH+Af4B/gH+Af4B/f4B/gH+Af4B/gICAgICAgICAgICAgICAgICAgIB/f39/f39/f39/f39/f3+Af4CAgICAgICAgICAgICAgICAf4B/gH9/f39/f39/f3+Af4CAgICAgICAgICAgICAgICAgICAf4B/gH9/f39/f39/gH+Af4CAgICAgICAgICAgICAgICAgICAgIB/gH+Af39/f39/f39/f3+Af4B/gICAgICAgICAgICAgICAgIB/gH9/f39/f39/f39/f4CAgICAgICAgICAgICAgICAgICAgIB/gH9/f39/f39/gH+Af4B/gICAgICAgICAgICAgICAgICAgICAgH9/f39/f39/f39/f3+Af4B/gH+AgICAgICAgICAgICAgICAf39/f39/f39/f39/gICAgICAgICAgICAgICAgICAgICAgICAgH+Af39/f39/f3+Af4B/gICAgICAgICAgICAgICAgICAgICAgH9/f39/f39/f39/f4CAf4B/gH+AgICAgICAgICAgICAgICAf39/f39/f39/f39/gH+AgICAgICAgICAgICAgICAgICAgICAgH+Af39/f39/f3+Af4CAgICAgICAgICAgICAgICAgICAgICAgICAf39/f39/f39/f4B/gIB/gH+AgICAgICAgICAgICAgICAgH9/f39/f39/f39/gICAgICAgICAgICAgICAgICAgICAgICAgICAf4B/gH+Af4CAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIB/gH9/f4CAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIB/gH+Af4B/gICAgICAgICAgICAgICAgICAgICAgICAgICAgIB/gH+Af4CAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgH+Af4B/gICAgICAgICAgICAgICAgICAgICAgICAgICAgIB/gH+Af4B/gICAgICAgICAgICAgICAgICAgICAgICAgICAgIB/gH+Af4CAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgH+Af4B/gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgH+Af4CAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIA=");
        }

        public static void PlaySound(string base64String)
        {
            byte[] audioBuffer = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(audioBuffer))
            {
                SoundPlayer player = new System.Media.SoundPlayer(ms);
                player.Play();
            }
        }

        private static DateTime NextProcessListUpdate = DateTime.UtcNow;

        public static Process[] _processList;

        public static Process[] ProcessList
        {
            get
            {
                if (DateTime.UtcNow > NextProcessListUpdate)
                    _processList = null;

                if (_processList == null)
                {
                    NextProcessListUpdate = DateTime.UtcNow.AddSeconds(1);
                    _processList = Process.GetProcesses();
                    return _processList;
                }

                return _processList;
            }
        }
        public static string AssemblyPath
        {
            get
            {
                if (_assemblyPath == null)
                    _assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return _assemblyPath;
            }
        }

        public static string GetInPath(string fileName)
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            var paths = path.Split(';');
            foreach (var p in paths)
            {
                var fullPath = Path.Combine(p, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        public static string GetNextToAssemblyPath(string fileName)
        {
            var path = Path.Combine(AssemblyPath, fileName);
            if (File.Exists(path))
                return path;
            return null;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void SetLastError(int errorCode);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandleW(IntPtr lpModuleName);

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibraryA(IntPtr lpModuleName);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern IntPtr
            FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string strClassName, string strWindowName);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

        public static int GetRandom(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
        public static string XmlSerialize(Object o)
        {
            var xmlSer = new XmlSerializer(o.GetType());
            var textWriter = new StringWriter();
            xmlSer.Serialize(textWriter, o);
            return textWriter.ToString();
        }

        public static void Dump(Object myObj)
        {
            foreach (var prop in myObj.GetType().GetProperties())
            {
                Console.WriteLine(prop.Name + ": " + prop.GetValue(myObj, null));
            }

            foreach (var field in myObj.GetType().GetFields())
            {
                Console.WriteLine(field.Name + ": " + field.GetValue(myObj));
            }
        }

        public static bool IsZipValid(string path)
        {
            try
            {
                using (var zipFile = ZipFile.OpenRead(path))
                {
                    var entries = zipFile.Entries;
                    return true;
                }
            }
            catch (InvalidDataException)
            {
                return false;
            }
        }

        public static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.ASCII.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }


        public static object XmlDeserialize(String s, Type t)
        {
            var xmlSer = new XmlSerializer(t);
            TextReader reader = new StringReader(s);
            var obj = xmlSer.Deserialize(reader);
            return obj;
        }

        public static void KillAllChildProcesses(int pid)
        {
            KillAllChildProcesses(pid, new List<string>());
        }

        public static void KillAllChildProcesses(int pid, List<string> exclude)
        {
            try
            {
                var proc = Process.GetProcessById(pid);
                var childProcs = proc.GetChildProcesses();
                foreach (var child in childProcs)
                {
                    if (exclude.Any(x => child.ProcessName.ToLower().StartsWith(x.ToLower())))
                    {
                        continue;
                    }

                    TaskKill(child.Id);
                    Debug.WriteLine($"Killing child. ProcessName [{child.ProcessName}] Id [{child.Id}]");
                }
            }
            catch { }
        }

        public static void TaskKill(int pid, bool createNoWindow = true)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo("taskkill", "/f /t /pid " + pid)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = createNoWindow,
                    UseShellExecute = false,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public static void RunInDirectory(string filename, bool waitforExit = true, string workingDirectory = null)
        {
            RunInDirectory(filename, String.Empty, waitforExit, workingDirectory);
        }

        public static void RunInDirectory(string filename, string parameters = "", bool waitforExit = true, string workingDirectory = null)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = !waitforExit;
            process.StartInfo.FileName = AssemblyPath + Path.DirectorySeparatorChar + filename;
            process.StartInfo.Arguments = parameters;
            process.StartInfo.WorkingDirectory = workingDirectory != null ? workingDirectory : AssemblyPath;
            process.StartInfo.CreateNoWindow = false;
            process.Start();

            if (waitforExit)
                process.WaitForExit();
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
        }


        //import log
        //import blue
        //import hashlib
        //import trinity
        //import sysinfo
        //import locale
        //import base64
        //import binascii
        //import codecs

        //try:
        //    varnames_20 = blue.sysinfo.GetNetworkAdapters()
        //    varnames_21 = []
        //    varnames_22 = []
        //    for varnames_23 in varnames_20:
        //        varnames_21.append(str(varnames_23.name if blue.sysinfo.isWine else varnames_23.uuid))
        //        varnames_22.append(varnames_23.macAddress)
        //        print(type(varnames_23.macAddress))
        //        print(str(tuple(varnames_23.macAddress)))
        //        print(binascii.hexlify(varnames_23.macAddress))



        //    varnames_21.sort()
        //    varnames_21 = str(tuple(varnames_21)).replace('(', '').replace(')', '').replace('}', '').replace('{', '').replace('-', '').replace(' ', '').replace("'", '').replace(',', '\n')
        //    varnames_22.sort()
        //    varnames_22 = str(tuple(varnames_22))
        //    print(varnames_22)
        //    varnames_22 = hashlib.md5(varnames_22).hexdigest()
        //except Exception as varnames_19:
        //    varnames_21 = None
        //    varnames_22 = None
        //    varnames_13('network adapters', varnames_19)
        //    sys.exc_clear()


        //print(varnames_22)



        //for i in range(0,256) :
        // #k = hex(i)[2:]
        // k = format(i, "02x")
        // b = binascii.unhexlify(k)
        //# b = bytes.fromhex(k)
        // print(str(tuple(b)))



        private static Dictionary<int, string> _reprLookup = new Dictionary<int, string>()
        {
            {0, @"\x00"}, {1, @"\x01"}, {2, @"\x02"}, {3, @"\x03"}, {4, @"\x04"}, {5, @"\x05"}, {6, @"\x06"}, {7, @"\x07"},
            {8, @"\x08"}, {9, @"\t"}, {10, @"\n"}, {11, @"\x0b"}, {12, @"\x0c"}, {13, @"\r"}, {14, @"\x0e"}, {15, @"\x0f"},
            {16, @"\x10"}, {17, @"\x11"}, {18, @"\x12"}, {19, @"\x13"}, {20, @"\x14"}, {21, @"\x15"}, {22, @"\x16"},
            {23, @"\x17"}, {24, @"\x18"}, {25, @"\x19"}, {26, @"\x1a"}, {27, @"\x1b"}, {28, @"\x1c"}, {29, @"\x1d"}, {30, @"\x1e"},
            {31, @"\x1f"}, {32, " "}, {33, "!"}, {34, "\""}, {35, "#"}, {36, "$"}, {37, "%"}, {38, "&"}, {39, "'"}, {40, "("},
            {41, ")"}, {42, "*"}, {43, "+"}, {44, ","}, {45, "-"}, {46, "."}, {47, "/"}, {48, "0"}, {49, "1"}, {50, "2"},
            {51, "3"}, {52, "4"}, {53, "5"}, {54, "6"}, {55, "7"}, {56, "8"}, {57, "9"}, {58, ":"}, {59, ";"}, {60, "<"},
            {61, "="}, {62, ">"}, {63, "?"}, {64, "@"}, {65, "A"}, {66, "B"}, {67, "C"}, {68, "D"}, {69, "E"}, {70, "F"},
            {71, "G"}, {72, "H"}, {73, "I"}, {74, "J"}, {75, "K"}, {76, "L"}, {77, "M"}, {78, "N"}, {79, "O"}, {80, "P"},
            {81, "Q"}, {82, "R"}, {83, "S"}, {84, "T"}, {85, "U"}, {86, "V"}, {87, "W"}, {88, "X"}, {89, "Y"}, {90, "Z"},
            {91, "["}, {92, @"\\"}, {93, "]"}, {94, "^"}, {95, "_"}, {96, "`"}, {97, "a"}, {98, "b"}, {99, "c"}, {100, "d"},
            {101, "e"}, {102, "f"}, {103, "g"}, {104, "h"}, {105, "i"}, {106, "j"}, {107, "k"}, {108, "l"}, {109, "m"},
            {110, "n"}, {111, "o"}, {112, "p"}, {113, "q"}, {114, "r"}, {115, "s"}, {116, "t"}, {117, "u"}, {118, "v"}, {119, "w"},
            {120, "x"}, {121, "y"}, {122, "z"}, {123, "{"}, {124, "|"}, {125, "}"}, {126, "~"}, {127, @"\x7f"},
            {128, @"\x80"}, {129, @"\x81"}, {130, @"\x82"}, {131, @"\x83"}, {132, @"\x84"}, {133, @"\x85"}, {134, @"\x86"},
            {135, @"\x87"}, {136, @"\x88"}, {137, @"\x89"}, {138, @"\x8a"}, {139, @"\x8b"}, {140, @"\x8c"}, {141, @"\x8d"}, {142, @"\x8e"},
            {143, @"\x8f"}, {144, @"\x90"}, {145, @"\x91"}, {146, @"\x92"}, {147, @"\x93"}, {148, @"\x94"}, {149, @"\x95"}, {150, @"\x96"},
            {151, @"\x97"}, {152, @"\x98"}, {153, @"\x99"}, {154, @"\x9a"}, {155, @"\x9b"}, {156, @"\x9c"}, {157, @"\x9d"}, {158, @"\x9e"},
            {159, @"\x9f"}, {160, @"\xa0"}, {161, @"\xa1"}, {162, @"\xa2"}, {163, @"\xa3"}, {164, @"\xa4"}, {165, @"\xa5"}, {166, @"\xa6"},
            {167, @"\xa7"}, {168, @"\xa8"}, {169, @"\xa9"}, {170, @"\xaa"}, {171, @"\xab"}, {172, @"\xac"}, {173, @"\xad"}, {174, @"\xae"},
            {175, @"\xaf"}, {176, @"\xb0"}, {177, @"\xb1"}, {178, @"\xb2"}, {179, @"\xb3"}, {180, @"\xb4"}, {181, @"\xb5"}, {182, @"\xb6"},
            {183, @"\xb7"}, {184, @"\xb8"}, {185, @"\xb9"}, {186, @"\xba"}, {187, @"\xbb"}, {188, @"\xbc"}, {189, @"\xbd"}, {190, @"\xbe"},
            {191, @"\xbf"}, {192, @"\xc0"}, {193, @"\xc1"}, {194, @"\xc2"}, {195, @"\xc3"}, {196, @"\xc4"}, {197, @"\xc5"}, {198, @"\xc6"},
            {199, @"\xc7"}, {200, @"\xc8"}, {201, @"\xc9"}, {202, @"\xca"}, {203, @"\xcb"}, {204, @"\xcc"}, {205, @"\xcd"}, {206, @"\xce"},
            {207, @"\xcf"}, {208, @"\xd0"}, {209, @"\xd1"}, {210, @"\xd2"}, {211, @"\xd3"}, {212, @"\xd4"}, {213, @"\xd5"}, {214, @"\xd6"},
            {215, @"\xd7"}, {216, @"\xd8"}, {217, @"\xd9"}, {218, @"\xda"}, {219, @"\xdb"}, {220, @"\xdc"}, {221, @"\xdd"}, {222, @"\xde"},
            {223, @"\xdf"}, {224, @"\xe0"}, {225, @"\xe1"}, {226, @"\xe2"}, {227, @"\xe3"}, {228, @"\xe4"}, {229, @"\xe5"}, {230, @"\xe6"},
            {231, @"\xe7"}, {232, @"\xe8"}, {233, @"\xe9"}, {234, @"\xea"}, {235, @"\xeb"}, {236, @"\xec"}, {237, @"\xed"}, {238, @"\xee"},
            {239, @"\xef"}, {240, @"\xf0"}, {241, @"\xf1"}, {242, @"\xf2"}, {243, @"\xf3"}, {244, @"\xf4"}, {245, @"\xf5"}, {246, @"\xf6"},
            {247, @"\xf7"}, {248, @"\xf8"}, {249, @"\xf9"}, {250, @"\xfa"}, {251, @"\xfb"}, {252, @"\xfc"}, {253, @"\xfd"}, {254, @"\xfe"},
            {255, @"\xff"},
        };

        public static string PyRepr(byte[] bytes)
        {
            var result = string.Empty;
            foreach (var b in bytes)
            {
                var intVal = Convert.ToInt32(b);
                var lookupRes = _reprLookup[intVal];
                //Debug.WriteLine($"intVal {intVal} lookupRes {lookupRes}");
                result += lookupRes;
            }
            return result;
        }


        public static string ExecCommand(string args)
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C " + args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.Start();
            process.BeginErrorReadLine();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        public static void Touch(string fileName)
        {
            var myFileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            myFileStream.Close();
            myFileStream.Dispose();
            File.SetLastWriteTimeUtc(fileName, DateTime.UtcNow);
        }

        public static Dictionary<IntPtr, string> GetVisibleWindows(int pid)
        {
            var dic = new Dictionary<IntPtr, string>();
            foreach (var hWnd in GetRootWindowsOfProcess(pid))
                if (IsWindowVisible(hWnd))
                {
                    var title = new StringBuilder(512);
                    GetWindowText(hWnd, title, 512);
                    if (dic.ContainsKey(hWnd))
                        continue;
                    dic.Add(hWnd, title.ToString().Trim());
                }
            return dic;
        }
        public static string GetWidowTitle(IntPtr hWnd)
        {
            var title = new StringBuilder(512);
            GetWindowText(hWnd, title, 512);
            return title.ToString().Trim();
        }

        public static string Sha256(string s)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s));
            foreach (var theByte in crypto)
                hash.Append(theByte.ToString("x2"));
            return hash.ToString();
        }

        public static string RemoveFirstLines(string text, int linesCount)
        {
            var lines = Regex.Split(text, "\r\n|\r|\n").Skip(linesCount);
            return String.Join(Environment.NewLine, lines.ToArray());
        }

        public static DateTime Unix2DateTime(UInt64 unix)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unix).ToUniversalTime();
            return dtDateTime;
        }

        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var obj = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return obj;
        }

        public static void MeasureTime(Action a, bool disableLog = false, string description = "", bool hideMicroSeconds = true)
        {
            try
            {
                using (new DisposableStopwatch(t =>
                {
                    if (!disableLog)
                    {
                        var desc = String.IsNullOrEmpty(description) ? "" : $"[{description}] ";
                        if (!hideMicroSeconds)
                            Console.WriteLine($"{desc}{1000000 * t.Ticks / Stopwatch.Frequency} µs elapsed.");
                        Console.WriteLine($"{desc}{(1000000 * t.Ticks / Stopwatch.Frequency) / 1000} ms elapsed.");
                    }
                }))
                {
                    a.Invoke();
                }
            }
            catch (Exception){}
        }

        public static T MeasureTime<T>(Func<T> a, bool disableLog = false)
        {
            using (new DisposableStopwatch(t =>
            {
                if (!disableLog)
                {
                    Console.WriteLine($"{1000000 * t.Ticks / Stopwatch.Frequency}  µs elapsed.");
                    Console.WriteLine($"{(1000000 * t.Ticks / Stopwatch.Frequency) / 1000} ms elapsed.");
                }
            }))
            {
                return a.Invoke();
            }
        }

        public static async Task WriteTextAsync(string filePath, string text, SemaphoreSlim sema)
        {
            try
            {
                byte[] encodedText = Encoding.UTF8.GetBytes(text);
                await sema.WaitAsync();
                try
                {
                    using (FileStream sourceStream = new FileStream(filePath,
                        FileMode.Append, FileAccess.Write, FileShare.None,
                        bufferSize: 4096, useAsync: true))
                    {

                        await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);

                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"WriteTextAsync Exception: {ex}");
                }
                finally
                {
                    sema.Release();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static List<CultureInfo> _cultures = new List<CultureInfo>()
        {
            CultureInfo.CurrentCulture,
            new CultureInfo("en-US", false), //US
            new CultureInfo("en-GB", false), //UK
            new CultureInfo("en-AU", false), //Australia
            new CultureInfo("en-DE", false), //Germany
        };

        public static DateTime? ParseDateTime(string s)
        {
            foreach (var culture in _cultures)
            {
                if (DateTime.TryParse(s, culture, DateTimeStyles.AdjustToUniversal, out var dtx))
                {
                    return dtx;
                }
            }

            if (DateTime.TryParse(s, out var dt))
                return dt;
            return null;
        }

        public static string ByteToHex(byte[] bytes)
        {
            var c = new char[bytes.Length * 2];
            int b;
            for (var i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("USER32.DLL")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, int lParam);

        private const int
            WM_PRINT = 0x317, PRF_CLIENT = 4,
            PRF_CHILDREN = 0x10, PRF_NON_CLIENT = 2,
            COMBINED_PRINTFLAGS = PRF_CLIENT | PRF_CHILDREN | PRF_NON_CLIENT;

        public static Bitmap PrintWindow(IntPtr hwnd)
        {
            GetWindowRect(hwnd, out var rc);
            Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            IntPtr hdcBitmap = gfxBmp.GetHdc();

            //SendMessage(hwnd, WM_PRINT, hdcBitmap, COMBINED_PRINTFLAGS);
            //const int flags = (0x10 | 0x4 | 0x20 | 0x2 | 0x8); // CHILDREN, CLIENT, OWNED, NONCLIENT, ERASEBKGND
            //Pinvokes.SendMessage(hwnd, 0x317, hdcBitmap, (IntPtr)flags);

            PrintWindow(hwnd, hdcBitmap, 0);
            //Pinvokes.SendMessage(hwnd, 0x000F, hdcBitmap, (IntPtr)0);

            gfxBmp.ReleaseHdc(hdcBitmap);
            gfxBmp.Dispose();

            return bmp;
        }

        public static Image CaptureWindow(IntPtr handle, int offsetX = 0, int offsetY = 0)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = Pinvokes.GetWindowDC(handle);
            // get the size
            Pinvokes.GetWindowRect(handle, out var windowRect);
            int width = windowRect.Width + offsetY;
            int height = windowRect.Height + offsetY;
            // create a device context we can copy to
            IntPtr hdcDest = Pinvokes.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = Pinvokes.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = Pinvokes.SelectObject(hdcDest, hBitmap);
            // bitblt over
            Pinvokes.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, Pinvokes.SRCCOPY);
            // restore selection
            Pinvokes.SelectObject(hdcDest, hOld);
            // clean up
            Pinvokes.DeleteDC(hdcDest);
            Pinvokes.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            Pinvokes.DeleteObject(hBitmap);
            return img;
        }

        public static Image ResizeImage(Image originalImage, int width, int height, ImageFormat format)
        {
            Image finalImage = new Bitmap(width, height);
            Graphics graphic = Graphics.FromImage(finalImage);
            graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            Rectangle rectangle = new Rectangle(0, 0, width, height);
            graphic.DrawImage(originalImage, rectangle);
            return finalImage;
        }

        public static Dictionary<IntPtr, string> GetInvisibleWindows(int pid)
        {
            var dic = new Dictionary<IntPtr, string>();
            var hWnd = IntPtr.Zero;
            uint threadID;
            uint currentProcId;
            do
            {
                currentProcId = 0;
                hWnd = FindWindowEx(IntPtr.Zero, hWnd, null, null);
                threadID = GetWindowThreadProcessId(hWnd, out currentProcId);
                if (pid == currentProcId)
                    if (!IsWindowVisible(hWnd))
                    {
                        var title = new StringBuilder(512);
                        GetWindowText(hWnd, title, 512);
                        if (title.ToString().Contains("GDI+ Window"))
                            continue;
                        if (title.ToString().Contains("NET-BroadcastEventWindow"))
                            continue;
                        if (dic.ContainsKey(hWnd))
                            continue;
                        dic.Add(hWnd, title.ToString().Trim());
                        //ShowWindow(hWnd, SW_RESTORE);
                    }
            } while (!hWnd.Equals(IntPtr.Zero));

            return dic;
        }

        public static List<IntPtr> GetRootWindowsOfProcess(int pid)
        {
            var rootWindows = GetChildWindows(IntPtr.Zero);
            var dsProcRootWindows = new List<IntPtr>();
            foreach (var hWnd in rootWindows)
            {
                GetWindowThreadProcessId(hWnd, out var lpdwProcessId);
                if (lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }

        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            var result = new List<IntPtr>();
            var listHandle = GCHandle.Alloc(result);
            try
            {
                var childProc = new Win32Callback(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            var gch = GCHandle.FromIntPtr(pointer);
            var list = gch.Target as List<IntPtr>;
            if (list == null)
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        public static IntPtr GetImportAddress(string module, string importedModule, string function)
        {
            var handle = GetModuleHandle(module);
            if (handle == IntPtr.Zero)
                return IntPtr.Zero;

            var address = GetThunk(handle, importedModule, function);
            if (address == IntPtr.Zero)
                return IntPtr.Zero;

            if (address == GetProcAddresFunc(importedModule, function))
                return IntPtr.Zero;

            return address;
        }

        private static IntPtr GetProcAddresFunc(string module, string function)
        {
            return GetProcAddress(GetModuleHandle(module), function);
        }

        public static IntPtr GetThunk(IntPtr moduleHandle, string intermodName, string funcName)
        {
            var idh = (IMAGE_DOS_HEADER)Marshal.PtrToStructure(moduleHandle, typeof(IMAGE_DOS_HEADER));
            if (!idh.isValid)
                return IntPtr.Zero;

            var inh32 = (IMAGE_NT_HEADERS32)Marshal.PtrToStructure(IntPtr.Add(moduleHandle, idh.e_lfanew), typeof(IMAGE_NT_HEADERS32));
            if (!inh32.isValid || inh32.OptionalHeader.ImportTable.VirtualAddress == 0)
                return IntPtr.Zero;

            var iidPtr = IntPtr.Add(moduleHandle, (int)inh32.OptionalHeader.ImportTable.VirtualAddress);
            if (iidPtr == IntPtr.Zero)
                return IntPtr.Zero;

            var iid = (IMAGE_IMPORT_DESCRIPTOR)Marshal.PtrToStructure(iidPtr, typeof(IMAGE_IMPORT_DESCRIPTOR));
            while (iid.Name != 0)
            {
                var iidName = Marshal.PtrToStringAnsi(IntPtr.Add(moduleHandle, (int)iid.Name));
                if (String.Compare(iidName, intermodName, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    iidPtr = IntPtr.Add(iidPtr, Marshal.SizeOf(typeof(IMAGE_IMPORT_DESCRIPTOR)));
                    iid = (IMAGE_IMPORT_DESCRIPTOR)Marshal.PtrToStructure(iidPtr, typeof(IMAGE_IMPORT_DESCRIPTOR));

                    continue;
                }

                // this probably won't work for 64-bit processes as the thunk data structure is different
                var itdPtr = IntPtr.Add(moduleHandle, (int)iid.FirstThunk);
                var oitdPtr = IntPtr.Add(moduleHandle, (int)iid.OriginalFirstThunk);
                while (itdPtr != IntPtr.Zero && oitdPtr != IntPtr.Zero)
                {
                    var itd = (IMAGE_THUNK_DATA)Marshal.PtrToStructure(itdPtr, typeof(IMAGE_THUNK_DATA));
                    var oitd = (IMAGE_THUNK_DATA)Marshal.PtrToStructure(oitdPtr, typeof(IMAGE_THUNK_DATA));

                    var iibnPtr = IntPtr.Add(moduleHandle, (int)oitd.AddressOfData);
                    var iibnName = Marshal.PtrToStringAnsi(IntPtr.Add(iibnPtr, Marshal.OffsetOf(typeof(IMAGE_IMPORT_BY_NAME), "Name").ToInt32()));
                    if (itd.Function == 0)
                        return IntPtr.Zero;

                    if (String.Compare(iibnName, funcName, StringComparison.OrdinalIgnoreCase) == 0)
                        return new IntPtr(itd.Function);

                    itdPtr = IntPtr.Add(itdPtr, Marshal.SizeOf(typeof(IMAGE_THUNK_DATA)));
                    oitdPtr = IntPtr.Add(oitdPtr, Marshal.SizeOf(typeof(IMAGE_THUNK_DATA)));
                }

                return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        public static void LoadLibrary(string libraryName, string module)
        {
            WCFClient.Instance.GetPipeProxy.RemoteLog(module + "Util.LoadLibrary(" + libraryName + ");");
            if (GetModuleHandle(libraryName) == IntPtr.Zero)
            {
                var lib = Marshal.StringToHGlobalAnsi(libraryName);
                try
                {
                    LoadLibraryA(lib);
                }
                finally
                {
                    Marshal.FreeHGlobal(lib);
                }
            }
        }

        public static void ExtractZipToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
            }
        }


        public static void DirectoryDelete(string path, bool recursive = false)
        {
            if (!Directory.Exists(path))
                return;

            if (!recursive)
                Array.ForEach(Directory.GetFiles(path), File.Delete);
            else
                Directory.Delete(path, true);
        }

        public static int NthIndexOf(string target, string value, int n)
        {
            var m = Regex.Match(target, "((" + Regex.Escape(value) + ").*?){" + n + "}");

            if (m.Success)
                return m.Groups[2].Captures[n - 1].Index;
            else
                return -1;
        }

        private static char HexChar(int value)
        {
            value &= 0xF;
            if (value >= 0 && value <= 9)
                return (char)('0' + value);
            return (char)('A' + (value - 10));
        }

        public static string HexDump(byte[] bytes)
        {
            if (bytes == null) return "<null>";
            var len = bytes.Length;
            var result = new StringBuilder((len + 15) / 16 * 78);
            var chars = new char[78];
            // fill all with blanks
            for (var i = 0; i < 75; i++)
                chars[i] = ' ';
            chars[76] = '\r';
            chars[77] = '\n';

            for (var i1 = 0; i1 < len; i1 += 16)
            {
                chars[0] = HexChar(i1 >> 28);
                chars[1] = HexChar(i1 >> 24);
                chars[2] = HexChar(i1 >> 20);
                chars[3] = HexChar(i1 >> 16);
                chars[4] = HexChar(i1 >> 12);
                chars[5] = HexChar(i1 >> 8);
                chars[6] = HexChar(i1 >> 4);
                chars[7] = HexChar(i1 >> 0);

                var offset1 = 11;
                var offset2 = 60;

                for (var i2 = 0; i2 < 16; i2++)
                {
                    if (i1 + i2 >= len)
                    {
                        chars[offset1] = ' ';
                        chars[offset1 + 1] = ' ';
                        chars[offset2] = ' ';
                    }
                    else
                    {
                        var b = bytes[i1 + i2];
                        chars[offset1] = HexChar(b >> 4);
                        chars[offset1 + 1] = HexChar(b);
                        chars[offset2] = b < 32 ? '·' : (char)b;
                    }
                    offset1 += i2 == 7 ? 4 : 3;
                    offset2++;
                }
                result.Append(chars);
            }
            return result.ToString();
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!dir.Exists)
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
        }

        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();

            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static void CheckCreateDirectorys(string windowsUserLogin)
        {
            var userDir = "C:\\Users\\" + windowsUserLogin + "\\";
            var path = new String[]
                {userDir + "Documents", userDir + "AppData\\Local\\Temp", userDir + "AppData\\Local\\Roaming", userDir + "AppData\\Local\\Temp"};

            foreach (var d in path)
                if (!Directory.Exists(d))
                    Directory.CreateDirectory(d);
        }

        public static byte[] Decompress(byte[] input)
        {
            var sourceStream = new MemoryStream(input, 2, input.Length - 2); // two bytes removed due zlid header
            var stream = new DeflateStream(sourceStream, CompressionMode.Decompress);
            return ReadAllBytes(stream);
        }

        public static byte[] ReadAllBytes(Stream source)
        {
            var readBuffer = new byte[4096];

            var totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = source.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readBuffer.Length)
                {
                    var nextByte = source.ReadByte();
                    if (nextByte != -1)
                    {
                        var temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            var buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }
            return buffer;
        }

        /// <summary>
        ///     A utility class to determine a process parent.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ParentProcessUtilities
        {
            // These members must match PROCESS_BASIC_INFORMATION
            internal IntPtr Reserved1;

            internal IntPtr PebBaseAddress;
            internal IntPtr Reserved2_0;
            internal IntPtr Reserved2_1;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;

            [DllImport("ntdll.dll")]
            private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass,
                ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

            /// <summary>
            ///     Gets the parent process of the current process.
            /// </summary>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess()
            {
                return GetParentProcess(Process.GetCurrentProcess().Handle);
            }

            /// <summary>
            ///     Gets the parent process of specified process.
            /// </summary>
            /// <param name="id">The process id.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(int id)
            {
                var process = Process.GetProcessById(id);
                return GetParentProcess(process.Handle);
            }

            /// <summary>
            ///     Gets the parent process of a specified process.
            /// </summary>
            /// <param name="handle">The process handle.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(IntPtr handle)
            {
                var pbi = new ParentProcessUtilities();
                int returnLength;
                var status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
                if (status != 0)
                    throw new Win32Exception(status);

                try
                {
                    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
                }
                catch (ArgumentException)
                {
                    // not found
                    return null;
                }
            }
        }

        public static Version GetRealWindowsVersion()
        {
            var osVersionInfo = new OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)) };
            if (RtlGetVersion(ref osVersionInfo) != 0)
            {
                return new Version(0, 0, 0);
            }
            return new Version(osVersionInfo.MajorVersion, osVersionInfo.MinorVersion, osVersionInfo.BuildNumber);
        }

        [SecurityCritical]
        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);
        [StructLayout(LayoutKind.Sequential)]
        internal struct OSVERSIONINFOEX
        {
            // The OSVersionInfoSize field must be set to Marshal.SizeOf(typeof(OSVERSIONINFOEX))
            internal int OSVersionInfoSize;
            internal int MajorVersion;
            internal int MinorVersion;
            internal int BuildNumber;
            internal int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string CSDVersion;
            internal ushort ServicePackMajor;
            internal ushort ServicePackMinor;
            internal short SuiteMask;
            internal byte ProductType;
            internal byte Reserved;
        }

        public int CpuUsageForProcess
        {
            get
            {
                return 0;
            }
        }

        public static async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            await Task.Delay(500).ConfigureAwait(false);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }

        #region Native structures

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DOS_HEADER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public char[] e_magic; // Magic number
            public UInt16 e_cblp; // Bytes on last page of file
            public UInt16 e_cp; // Pages in file
            public UInt16 e_crlc; // Relocations
            public UInt16 e_cparhdr; // Size of header in paragraphs
            public UInt16 e_minalloc; // Minimum extra paragraphs needed
            public UInt16 e_maxalloc; // Maximum extra paragraphs needed
            public UInt16 e_ss; // Initial (relative) SS value
            public UInt16 e_sp; // Initial SP value
            public UInt16 e_csum; // Checksum
            public UInt16 e_ip; // Initial IP value
            public UInt16 e_cs; // Initial (relative) CS value
            public UInt16 e_lfarlc; // File address of relocation table
            public UInt16 e_ovno; // Overlay number
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public UInt16[] e_res1; // Reserved words
            public UInt16 e_oemid; // OEM identifier (for e_oeminfo)
            public UInt16 e_oeminfo; // OEM information; e_oemid specific
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public UInt16[] e_res2; // Reserved words
            public Int32 e_lfanew; // File address of new exe header

            private string _e_magic => new string(e_magic);

            public bool isValid => _e_magic == "MZ";
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public UInt32 VirtualAddress;
            public UInt32 Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_FILE_HEADER
        {
            public UInt16 Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public UInt16 Characteristics;
        }

        public enum MachineType : ushort
        {
            Native = 0,
            I386 = 0x014c,
            Itanium = 0x0200,
            x64 = 0x8664
        }

        public enum MagicType : ushort
        {
            IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
            IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b
        }

        public enum SubSystemType : ushort
        {
            IMAGE_SUBSYSTEM_UNKNOWN = 0,
            IMAGE_SUBSYSTEM_NATIVE = 1,
            IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,
            IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,
            IMAGE_SUBSYSTEM_POSIX_CUI = 7,
            IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9,
            IMAGE_SUBSYSTEM_EFI_APPLICATION = 10,
            IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,
            IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12,
            IMAGE_SUBSYSTEM_EFI_ROM = 13,
            IMAGE_SUBSYSTEM_XBOX = 14
        }

        public enum DllCharacteristicsType : ushort
        {
            RES_0 = 0x0001,
            RES_1 = 0x0002,
            RES_2 = 0x0004,
            RES_3 = 0x0008,
            IMAGE_DLL_CHARACTERISTICS_DYNAMIC_BASE = 0x0040,
            IMAGE_DLL_CHARACTERISTICS_FORCE_INTEGRITY = 0x0080,
            IMAGE_DLL_CHARACTERISTICS_NX_COMPAT = 0x0100,
            IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = 0x0200,
            IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400,
            IMAGE_DLLCHARACTERISTICS_NO_BIND = 0x0800,
            RES_4 = 0x1000,
            IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000,
            IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            [FieldOffset(0)] public MagicType Magic;

            [FieldOffset(2)] public byte MajorLinkerVersion;

            [FieldOffset(3)] public byte MinorLinkerVersion;

            [FieldOffset(4)] public uint SizeOfCode;

            [FieldOffset(8)] public uint SizeOfInitializedData;

            [FieldOffset(12)] public uint SizeOfUninitializedData;

            [FieldOffset(16)] public uint AddressOfEntryPoint;

            [FieldOffset(20)] public uint BaseOfCode;

            // PE32 contains this additional field
            [FieldOffset(24)] public uint BaseOfData;

            [FieldOffset(28)] public uint ImageBase;

            [FieldOffset(32)] public uint SectionAlignment;

            [FieldOffset(36)] public uint FileAlignment;

            [FieldOffset(40)] public ushort MajorOperatingSystemVersion;

            [FieldOffset(42)] public ushort MinorOperatingSystemVersion;

            [FieldOffset(44)] public ushort MajorImageVersion;

            [FieldOffset(46)] public ushort MinorImageVersion;

            [FieldOffset(48)] public ushort MajorSubsystemVersion;

            [FieldOffset(50)] public ushort MinorSubsystemVersion;

            [FieldOffset(52)] public uint Win32VersionValue;

            [FieldOffset(56)] public uint SizeOfImage;

            [FieldOffset(60)] public uint SizeOfHeaders;

            [FieldOffset(64)] public uint CheckSum;

            [FieldOffset(68)] public SubSystemType Subsystem;

            [FieldOffset(70)] public DllCharacteristicsType DllCharacteristics;

            [FieldOffset(72)] public uint SizeOfStackReserve;

            [FieldOffset(76)] public uint SizeOfStackCommit;

            [FieldOffset(80)] public uint SizeOfHeapReserve;

            [FieldOffset(84)] public uint SizeOfHeapCommit;

            [FieldOffset(88)] public uint LoaderFlags;

            [FieldOffset(92)] public uint NumberOfRvaAndSizes;

            [FieldOffset(96)] public IMAGE_DATA_DIRECTORY ExportTable;

            [FieldOffset(104)] public IMAGE_DATA_DIRECTORY ImportTable;

            [FieldOffset(112)] public IMAGE_DATA_DIRECTORY ResourceTable;

            [FieldOffset(120)] public IMAGE_DATA_DIRECTORY ExceptionTable;

            [FieldOffset(128)] public IMAGE_DATA_DIRECTORY CertificateTable;

            [FieldOffset(136)] public IMAGE_DATA_DIRECTORY BaseRelocationTable;

            [FieldOffset(144)] public IMAGE_DATA_DIRECTORY Debug;

            [FieldOffset(152)] public IMAGE_DATA_DIRECTORY Architecture;

            [FieldOffset(160)] public IMAGE_DATA_DIRECTORY GlobalPtr;

            [FieldOffset(168)] public IMAGE_DATA_DIRECTORY TLSTable;

            [FieldOffset(176)] public IMAGE_DATA_DIRECTORY LoadConfigTable;

            [FieldOffset(184)] public IMAGE_DATA_DIRECTORY BoundImport;

            [FieldOffset(192)] public IMAGE_DATA_DIRECTORY IAT;

            [FieldOffset(200)] public IMAGE_DATA_DIRECTORY DelayImportDescriptor;

            [FieldOffset(208)] public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

            [FieldOffset(216)] public IMAGE_DATA_DIRECTORY Reserved;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_NT_HEADERS32
        {
            [FieldOffset(0)][MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public char[] Signature;

            [FieldOffset(4)] public IMAGE_FILE_HEADER FileHeader;

            [FieldOffset(24)] public IMAGE_OPTIONAL_HEADER32 OptionalHeader;

            private string _Signature => new string(Signature);

            public bool isValid => _Signature == "PE\0\0" && OptionalHeader.Magic == MagicType.IMAGE_NT_OPTIONAL_HDR32_MAGIC;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_IMPORT_BY_NAME
        {
            public short Hint;
            public byte Name;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_IMPORT_DESCRIPTOR
        {
            #region union

            /// <summary>
            ///     C# doesn't really support unions, but they can be emulated by a field offset 0
            /// </summary>
            [FieldOffset(0)] public uint Characteristics; // 0 for terminating null import descriptor

            [FieldOffset(0)] public uint OriginalFirstThunk; // RVA to original unbound IAT (PIMAGE_THUNK_DATA)

            #endregion

            [FieldOffset(4)] public uint TimeDateStamp;
            [FieldOffset(8)] public uint ForwarderChain;
            [FieldOffset(12)] public uint Name;
            [FieldOffset(16)] public uint FirstThunk;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_THUNK_DATA
        {
            [FieldOffset(0)] public uint ForwarderString; // PBYTE
            [FieldOffset(0)] public uint Function; // PDWORD
            [FieldOffset(0)] public uint Ordinal;
            [FieldOffset(0)] public uint AddressOfData; // PIMAGE_IMPORT_BY_NAME
        }

        #endregion
    }
}