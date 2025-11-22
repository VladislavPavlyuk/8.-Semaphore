using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace SemaphoreApp
{
    public partial class Form1 : Form
    {
        private volatile Semaphore semaphore;
        private int threadCounter = 0;
        private readonly object lockObject = new object();
        private readonly object semaphoreLock = new object();
        private readonly Dictionary<int, ThreadInfo> threadInfos = new Dictionary<int, ThreadInfo>();
        private int currentSemaphoreSlots = 2;
        private int availableSlots = 2;

        public Form1()
        {
            InitializeComponent();
            semaphore = new Semaphore(currentSemaphoreSlots, 100); // Use large max capacity
        }

        private void btnCreateThread_Click(object sender, EventArgs e)
        {
            threadCounter++;
            int threadId = threadCounter;
            
            ThreadInfo threadInfo = new ThreadInfo
            {
                Id = threadId,
                Name = $"Thread {threadId}",
                Counter = 0,
                IsWorking = false,
                CancellationTokenSource = new CancellationTokenSource()
            };

            lock (lockObject)
            {
                threadInfos[threadId] = threadInfo;
            }

            // Add to created threads list
            if (InvokeRequired)
            {
                Invoke(new Action(() => listBoxCreated.Items.Add(threadInfo.DisplayText)));
            }
            else
            {
                listBoxCreated.Items.Add(threadInfo.DisplayText);
            }

            // Start counter thread that increments every second
            Thread counterThread = new Thread(() => CounterThread(threadId));
            counterThread.IsBackground = true;
            counterThread.Start();
        }

        private void listBoxCreated_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxCreated.SelectedItem == null) return;

            string selectedText = listBoxCreated.SelectedItem.ToString();
            int threadId = ExtractThreadId(selectedText);

            if (threadId > 0 && threadInfos.ContainsKey(threadId))
            {
                ThreadInfo threadInfo = threadInfos[threadId];
                
                // Remove from created list
                listBoxCreated.Items.Remove(selectedText);
                
                // Add to waiting list
                listBoxWaiting.Items.Add(threadInfo.DisplayText);
                
                // Check if we can immediately move to working
                EnsureWorkingThreadsCount();
            }
        }

        private void WaitForSemaphoreSlot(int threadId)
        {
            // Wait for semaphore slot
            semaphore.WaitOne();

            // Check capacity limit and if thread was cancelled
            lock (semaphoreLock)
            {
                // Check if we're within capacity limits
                int currentlyWorking = GetWorkingThreadsCount();
                if (currentlyWorking >= currentSemaphoreSlots)
                {
                    // At or over capacity, release and return
                    semaphore.Release();
                    return;
                }
                // We're within capacity, reserve the slot
                availableSlots--;
            }

            lock (lockObject)
            {
                if (!threadInfos.ContainsKey(threadId) || threadInfos[threadId].CancellationTokenSource.IsCancellationRequested)
                {
                    lock (semaphoreLock)
                    {
                        availableSlots++;
                        semaphore.Release();
                    }
                    return;
                }

                ThreadInfo threadInfo = threadInfos[threadId];
                
                // Double-check if still in waiting list (might have been removed due to slot decrease)
                bool stillWaiting = false;
                if (InvokeRequired)
                {
                    Invoke(new Action(() => stillWaiting = listBoxWaiting.Items.Contains(threadInfo.DisplayText)));
                }
                else
                {
                    stillWaiting = listBoxWaiting.Items.Contains(threadInfo.DisplayText);
                }

                if (!stillWaiting)
                {
                    lock (semaphoreLock)
                    {
                        availableSlots++;
                        semaphore.Release();
                    }
                    return;
                }

                threadInfo.IsWorking = true;

                // Move from waiting to working list
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        listBoxWaiting.Items.Remove(threadInfo.DisplayText);
                        listBoxWorking.Items.Add(threadInfo.DisplayText);
                    }));
                }
                else
                {
                    listBoxWaiting.Items.Remove(threadInfo.DisplayText);
                    listBoxWorking.Items.Add(threadInfo.DisplayText);
                }

                // Start working thread
                Thread workThread = new Thread(() => WorkThread(threadId));
                workThread.IsBackground = true;
                workThread.Start();
            }
        }

        private int GetWorkingThreadsCount()
        {
            if (InvokeRequired)
            {
                int count = 0;
                Invoke(new Action(() => count = listBoxWorking.Items.Count));
                return count;
            }
            return listBoxWorking.Items.Count;
        }

        private int GetWaitingThreadsCount()
        {
            if (InvokeRequired)
            {
                int count = 0;
                Invoke(new Action(() => count = listBoxWaiting.Items.Count));
                return count;
            }
            return listBoxWaiting.Items.Count;
        }

        private void EnsureWorkingThreadsCount()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnsureWorkingThreadsCount()));
                return;
            }

            lock (semaphoreLock)
            {
                int currentlyWorking = listBoxWorking.Items.Count;
                int needed = currentSemaphoreSlots - currentlyWorking;

                if (needed > 0)
                {
                    // Need to add threads from waiting list
                    int waitingCount = listBoxWaiting.Items.Count;
                    int threadsToAdd = Math.Min(needed, waitingCount);
                    
                    for (int i = 0; i < threadsToAdd; i++)
                    {
                        if (listBoxWaiting.Items.Count > 0)
                        {
                            string waitingText = listBoxWaiting.Items[0].ToString();
                            int threadId = ExtractThreadId(waitingText);
                            if (threadId > 0 && threadInfos.ContainsKey(threadId))
                            {
                                Thread waitThread = new Thread(() => WaitForSemaphoreSlot(threadId));
                                waitThread.IsBackground = true;
                                waitThread.Start();
                            }
                        }
                    }
                }
                else if (needed < 0)
                {
                    // Need to remove excess threads (oldest first)
                    int threadsToRemove = -needed;
                    for (int i = 0; i < threadsToRemove && listBoxWorking.Items.Count > 0; i++)
                    {
                        string workingText = listBoxWorking.Items[0].ToString();
                        int threadId = ExtractThreadId(workingText);
                        if (threadId > 0 && threadInfos.ContainsKey(threadId))
                        {
                            ThreadInfo threadInfo;
                            lock (lockObject)
                            {
                                if (!threadInfos.ContainsKey(threadId)) continue;
                                threadInfo = threadInfos[threadId];
                            }
                            
                            threadInfo.CancellationTokenSource.Cancel();
                            threadInfo.IsWorking = false;
                            
                            listBoxWorking.Items.RemoveAt(0);
                            
                            lock (lockObject)
                            {
                                threadInfos.Remove(threadId);
                            }
                            
                            availableSlots++;
                            semaphore.Release();
                        }
                    }
                }
            }
        }

        private void CounterThread(int threadId)
        {
            ThreadInfo threadInfo;
            lock (lockObject)
            {
                if (!threadInfos.ContainsKey(threadId)) return;
                threadInfo = threadInfos[threadId];
            }

            while (!threadInfo.CancellationTokenSource.Token.IsCancellationRequested)
            {
                Thread.Sleep(1000); // Wait 1 second

                if (threadInfo.CancellationTokenSource.Token.IsCancellationRequested)
                    break;

                lock (lockObject)
                {
                    if (!threadInfos.ContainsKey(threadId)) break;
                    threadInfo.Counter++;
                }

                // Update display in all lists
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateThreadDisplay(threadId)));
                }
                else
                {
                    UpdateThreadDisplay(threadId);
                }
            }
        }

        private void UpdateThreadDisplay(int threadId)
        {
            lock (lockObject)
            {
                if (!threadInfos.ContainsKey(threadId)) return;
                ThreadInfo threadInfo = threadInfos[threadId];
                string displayText = threadInfo.DisplayText;
                string searchPattern = $"Thread {threadId}";

                // Update in created list
                for (int i = 0; i < listBoxCreated.Items.Count; i++)
                {
                    string item = listBoxCreated.Items[i].ToString();
                    if (item.StartsWith(searchPattern))
                    {
                        listBoxCreated.Items[i] = displayText;
                        break;
                    }
                }

                // Update in waiting list
                for (int i = 0; i < listBoxWaiting.Items.Count; i++)
                {
                    string item = listBoxWaiting.Items[i].ToString();
                    if (item.StartsWith(searchPattern))
                    {
                        listBoxWaiting.Items[i] = displayText;
                        break;
                    }
                }

                // Update in working list
                for (int i = 0; i < listBoxWorking.Items.Count; i++)
                {
                    string item = listBoxWorking.Items[i].ToString();
                    if (item.StartsWith(searchPattern))
                    {
                        listBoxWorking.Items[i] = displayText;
                        break;
                    }
                }
            }
        }

        private void WorkThread(int threadId)
        {
            ThreadInfo threadInfo;
            lock (lockObject)
            {
                if (!threadInfos.ContainsKey(threadId)) return;
                threadInfo = threadInfos[threadId];
            }

            // Work thread just waits while the thread is working
            // Counter is incremented by CounterThread
            while (!threadInfo.CancellationTokenSource.Token.IsCancellationRequested && threadInfo.IsWorking)
            {
                Thread.Sleep(100);
            }

            // Release semaphore slot
            lock (semaphoreLock)
            {
                availableSlots++;
                semaphore.Release();
            }

            // Ensure working threads count matches slots
            EnsureWorkingThreadsCount();
        }

        private void listBoxWorking_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxWorking.SelectedItem == null) return;

            string selectedText = listBoxWorking.SelectedItem.ToString();
            int threadId = ExtractThreadId(selectedText);

            if (threadId > 0 && threadInfos.ContainsKey(threadId))
            {
                ThreadInfo threadInfo;
                lock (lockObject)
                {
                    if (!threadInfos.ContainsKey(threadId)) return;
                    threadInfo = threadInfos[threadId];
                }
                
                // Cancel the thread
                threadInfo.CancellationTokenSource.Cancel();
                threadInfo.IsWorking = false;

                // Remove from working list
                listBoxWorking.Items.Remove(selectedText);

                // Remove thread info
                lock (lockObject)
                {
                    threadInfos.Remove(threadId);
                }

                // Release semaphore slot
                lock (semaphoreLock)
                {
                    availableSlots++;
                    semaphore.Release();
                }

                // Ensure working threads count matches slots
                EnsureWorkingThreadsCount();
            }
        }

        private void numericUpDownSlots_ValueChanged(object sender, EventArgs e)
        {
            int newSlots = (int)numericUpDownSlots.Value;
            int difference = newSlots - currentSemaphoreSlots;

            lock (semaphoreLock)
            {
                currentSemaphoreSlots = newSlots;

                if (difference > 0)
                {
                    // Increase semaphore slots - release additional slots
                    int additionalSlots = difference;
                    availableSlots += additionalSlots;
                    
                    // Release semaphore slots to allow waiting threads
                    Semaphore currentSemaphore = semaphore;
                    for (int i = 0; i < additionalSlots; i++)
                    {
                        currentSemaphore.Release();
                    }
                }
                else if (difference < 0)
                {
                    // Decrease semaphore slots - adjust available slots
                    availableSlots = Math.Max(0, availableSlots + difference);
                }

                // Ensure working threads count matches new slot count
                EnsureWorkingThreadsCount();
            }
        }

        private int ExtractThreadId(string displayText)
        {
            // Format: "Thread X - Counter: Y"
            if (string.IsNullOrEmpty(displayText)) return 0;
            
            int dashIndex = displayText.IndexOf(" - ");
            if (dashIndex < 0) return 0;
            
            string threadPart = displayText.Substring(0, dashIndex);
            string[] parts = threadPart.Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int id))
            {
                return id;
            }
            
            return 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel all threads
            lock (lockObject)
            {
                foreach (var threadInfo in threadInfos.Values)
                {
                    threadInfo.CancellationTokenSource.Cancel();
                }
            }

            // Wait a bit for threads to finish
            Thread.Sleep(200);
            
            // Dispose semaphore
            lock (semaphoreLock)
            {
                semaphore?.Dispose();
            }
        }

        private class ThreadInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Counter { get; set; }
            public bool IsWorking { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }

            public string DisplayText => $"{Name} - Counter: {Counter}";
        }
    }
}

