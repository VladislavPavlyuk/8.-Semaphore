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
                
                // Try to acquire semaphore slot
                Thread waitThread = new Thread(() => WaitForSemaphoreSlot(threadId));
                waitThread.IsBackground = true;
                waitThread.Start();
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
                int currentlyWorking = currentSemaphoreSlots - availableSlots;
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

        private void WorkThread(int threadId)
        {
            ThreadInfo threadInfo;
            lock (lockObject)
            {
                if (!threadInfos.ContainsKey(threadId)) return;
                threadInfo = threadInfos[threadId];
            }

            while (!threadInfo.CancellationTokenSource.Token.IsCancellationRequested && threadInfo.IsWorking)
            {
                Thread.Sleep(1000); // Wait 1 second

                if (threadInfo.CancellationTokenSource.Token.IsCancellationRequested)
                    break;

                lock (lockObject)
                {
                    if (!threadInfos.ContainsKey(threadId)) break;
                    threadInfo.Counter++;
                }
                
                // Update display in working list
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        lock (lockObject)
                        {
                            if (!threadInfos.ContainsKey(threadId)) return;
                            int index = listBoxWorking.Items.IndexOf(threadInfo.DisplayText);
                            if (index >= 0)
                            {
                                listBoxWorking.Items[index] = threadInfo.DisplayText;
                            }
                        }
                    }));
                }
                else
                {
                    lock (lockObject)
                    {
                        if (!threadInfos.ContainsKey(threadId)) return;
                        int index = listBoxWorking.Items.IndexOf(threadInfo.DisplayText);
                        if (index >= 0)
                        {
                            listBoxWorking.Items[index] = threadInfo.DisplayText;
                        }
                    }
                }
            }

            // Release semaphore slot
            lock (semaphoreLock)
            {
                availableSlots++;
                semaphore.Release();
            }

            // Try to move next waiting thread to working
            if (listBoxWaiting.Items.Count > 0)
            {
                string nextWaitingText = listBoxWaiting.Items[0].ToString();
                int nextThreadId = ExtractThreadId(nextWaitingText);
                if (nextThreadId > 0 && threadInfos.ContainsKey(nextThreadId))
                {
                    Thread nextWaitThread = new Thread(() => WaitForSemaphoreSlot(nextThreadId));
                    nextWaitThread.IsBackground = true;
                    nextWaitThread.Start();
                }
            }
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

                // Try to move next waiting thread to working
                if (listBoxWaiting.Items.Count > 0)
                {
                    string nextWaitingText = listBoxWaiting.Items[0].ToString();
                    int nextThreadId = ExtractThreadId(nextWaitingText);
                    if (nextThreadId > 0 && threadInfos.ContainsKey(nextThreadId))
                    {
                        Thread nextWaitThread = new Thread(() => WaitForSemaphoreSlot(nextThreadId));
                        nextWaitThread.IsBackground = true;
                        nextWaitThread.Start();
                    }
                }
            }
        }

        private void numericUpDownSlots_ValueChanged(object sender, EventArgs e)
        {
            int newSlots = (int)numericUpDownSlots.Value;
            int difference = newSlots - currentSemaphoreSlots;

            lock (semaphoreLock)
            {
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

                    // Move waiting threads to working if slots available
                    int slotsToFill = Math.Min(additionalSlots, listBoxWaiting.Items.Count);
                    for (int i = 0; i < slotsToFill; i++)
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
                else if (difference < 0)
                {
                    // Decrease semaphore slots - remove oldest working threads first
                    int threadsToRemove = Math.Min(-difference, listBoxWorking.Items.Count);
                    for (int i = 0; i < threadsToRemove; i++)
                    {
                        if (listBoxWorking.Items.Count > 0)
                        {
                            string workingText = listBoxWorking.Items[0].ToString();
                            int threadId = ExtractThreadId(workingText);
                            if (threadId > 0 && threadInfos.ContainsKey(threadId))
                            {
                                ThreadInfo threadInfo = threadInfos[threadId];
                                threadInfo.CancellationTokenSource.Cancel();
                                threadInfo.IsWorking = false;
                                
                                listBoxWorking.Items.RemoveAt(0);
                                
                                lock (lockObject)
                                {
                                    threadInfos.Remove(threadId);
                                }
                            }
                        }
                    }

                    // If still need to reduce more, remove from waiting list (oldest first)
                    int remainingReduction = -difference - threadsToRemove;
                    for (int i = 0; i < remainingReduction && listBoxWaiting.Items.Count > 0; i++)
                    {
                        string waitingText = listBoxWaiting.Items[0].ToString();
                        int threadId = ExtractThreadId(waitingText);
                        if (threadId > 0 && threadInfos.ContainsKey(threadId))
                        {
                            ThreadInfo threadInfo = threadInfos[threadId];
                            threadInfo.CancellationTokenSource.Cancel();
                            
                            listBoxWaiting.Items.RemoveAt(0);
                            
                            lock (lockObject)
                            {
                                threadInfos.Remove(threadId);
                            }
                        }
                    }

                    // Adjust available slots
                    availableSlots = Math.Max(0, availableSlots + difference);
                }

                currentSemaphoreSlots = newSlots;
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

