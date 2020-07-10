using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Nest;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Practice.AmazonPrep.BST;

namespace Practice
{
    public class Program
    {
        // Do not change the name of this class
        public class StringMap<TValue> : IStringMap<TValue>
            where TValue : class
        {

            IDictionary<string, TValue> dict = new Dictionary<string, TValue>();

            /// <summary> Returns number of elements in a map</summary>
            public int Count => dict.Count;

            /// <summary>
            /// If <c>GetValue</c> method is called but a given key is not in a map then <c>DefaultValue</c> is returned.
            /// </summary>
            // Do not change this property
            public TValue DefaultValue { get; set; }

            /// <summary>
            /// Adds a given key and value to a map.
            /// If the given key already exists in a map, then the value associated with this key should be overriden.
            /// </summary>
            /// <returns>true if the value for the key was overriden otherwise false</returns>
            /// <exception cref="System.ArgumentNullException">If the key is null</exception>
            /// <exception cref="System.ArgumentException">If the key is an empty string</exception>
            /// <exception cref="System.ArgumentNullException">If the value is null</exception>
            public bool AddElement(string key, TValue value)
            {
                if (key == null)
                {
                    throw new ArgumentNullException(key);
                }

                if(key.Length==0)
                {
                    throw new ArgumentException("key cannot be empty");
                }

                if(value == null)
                {
                    throw new ArgumentNullException("value cannot be null");
                }

                if (dict.ContainsKey(key))
                {
                    dict[key] = value;
                    return true;
                }

                dict.Add(key, value);
                return false;
            }

            /// <summary>
            /// Removes a given key and associated value from a map.
            /// </summary>
            /// <returns>true if the key was in the map and was removed otherwise false</returns>
            /// <exception cref="System.ArgumentNullException">If the key is null</exception>
            /// <exception cref="System.ArgumentException">If the key is an empty string</exception>
            public bool RemoveElement(string key)
            {
                if (key == null)
                {
                    throw new ArgumentNullException(key);
                }

                if (key.Length == 0)
                {
                    throw new ArgumentException("key cannot be empty");
                }

                if (dict.ContainsKey(key))
                {
                    dict.Remove(key);
                }
                return false;
            }


            /// <summary>
            /// Returns the value associated with a given key.
            /// </summary>
            /// <returns>The value associated with a given key or <c>DefaultValue</c> if the key does not exist in a map</returns>
            /// <exception cref="System.ArgumentNullException">If a key is null</exception>
            /// <exception cref="System.ArgumentException">If a key is an empty string</exception>
            public TValue GetValue(string key)
            {
                if (key == null)
                {
                    throw new ArgumentNullException(key);
                }

                if (key.Length == 0)
                {
                    throw new ArgumentException("key cannot be empty");
                }

                if (dict.ContainsKey(key))
                {
                    dict.TryGetValue(key, out TValue value);
                    return value;
                }
                return DefaultValue;
            }
        }

        public static int getVal(int[] A)
        {

            var points = new HashSet<int>();
            int max = 0;

            foreach(var i in A)
            {
                points.Add(i);
            }

            for (int i = 0; i < A.Length; ++i)
            {
                
                if (A[i] > max)
                {
                    if (points.Contains(A[i] * -1))
                    {
                        max = A[i];
                    }
                }
            }
            return max;
        }

        public static int solve(int[] A)
        {
            int max = int.MinValue;
            foreach(int i in A)
            {
                if (i > -10 && i<10)
                {
                    if (i > max)
                    {
                        max = i;
                    }
                }
            }
            return max;
        }


        public static List<V> ListAllBlobs<T, V>(Expression<Func<T, V>> expression, string containerName, string prefix)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("YourConnectionString;");

            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            var list = container.ListBlobs(prefix: prefix, useFlatBlobListing: true);

            List<V> data = list.OfType<T>().Select(expression.Compile()).ToList();
            return data;
        }

        private static readonly int[] LookupTable =
        Enumerable.Range(0, 256).Select(CountBits).ToArray();

        private static int CountBits(int value)
        {
            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                count += (value >> i) & 1;
            }
            return count;
        }

        public static int CountBitsAfterXor(byte[] array)
        {
            int xor = 0;
            foreach (byte b in array)
            {
                xor ^= b;
            }
            return LookupTable[xor];
        }

        public static void Main(string[] args)
        {
            // https://inversionrecruitment.blob.core.windows.net/find-the-joker
            // Get a reference to a container that's available for anonymous access.
            BlobContainerClient container = new BlobContainerClient
                (new Uri(@"https://inversionrecruitment.blob.core.windows.net/find-the-joker"));

            // List blobs in the container.
            // Note this is only possible when the container supports full public read access.
            var blobs = container.GetBlobs();
            decimal jokerValue = 0;
            decimal others = 0;
            var count = 0;
            foreach (BlobItem blobItem in blobs)
            {
                var length = blobItem.Properties.ContentLength;

                BlockBlobClient blob = new BlockBlobClient(container.GetBlockBlobClient(blobItem.Name).Uri);

                
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetStringAsync(container.GetBlockBlobClient(blobItem.Name).Uri).Result;
                    // Convert the string into a byte[].
                    byte[] asciiBytes = Encoding.ASCII.GetBytes(response);

                    decimal sum = 0;
                    
                    foreach(var item in asciiBytes)
                    {
                        sum += item;
                    }

                    //Console.WriteLine($"Values Before: Others =  {others}, Joker = {jokerValue}");
                    if(length!= 11000)
                    {
                        //Console.WriteLine(++count);
                        others += sum;
                    }
                    else
                    {
                        Console.WriteLine("found the joker guy");
                        Console.WriteLine(container.GetBlockBlobClient(blobItem.Name).Uri);
                        jokerValue = sum;
                    }

                    Console.WriteLine($"Values After: Others =  {others}, Joker = {jokerValue}");

                }
            }

            Console.WriteLine(jokerValue * others);
            //var list = container.ListBlobs(prefix: prefix, useFlatBlobListing: true);


            // Array to be updated 
            //var param = new int[] { 3, 2, -2, 5, -3};
            ////IsPrime();
            //var res = getVal(param);

            //var cusMe = new StringMap<string>();
            //var count = cusMe.Count;
            //var add = cusMe.AddElement("obi", "boy");
            //var again = cusMe.AddElement("ada", "girl");
            //var final = cusMe.AddElement("obi", "override");
            //var obi = cusMe.GetValue("obi");

            //Expression<Func<int, int, int>> adder = (x, y) => x + y;

            //Console.WriteLine(adder);

            //var date = DateTime.Now;

            //var message = "";
            //var sec = " ";
            //var first = string.IsNullOrEmpty(sec);
            //var second = string.IsNullOrEmpty(message);
            //var third = message.Length;
            //var fourth = sec.Length;

            //solution(new int[] { 1,2,3},new string[] { "square", "accumulate" });

            //IsPrime(7);
            ////IsPrime();

            //Node root = new Node(50);
            //insert(root, 23);
            //insert(root, 12);
            //insert(root, 19);
            //insert(root, 54);
            //insert(root, 9);
            //insert(root, 14);
            //insert(root, 67);
            //insert(root, 76);
            //insert(root, 72);
            //////nums.Insert(17);
            ////nums.Insert(23);
            ////nums.Insert(12);
            ////nums.Insert(19);
            ////nums.Insert(54);
            ////nums.Insert(9);
            ////nums.Insert(14);
            ////nums.Insert(67);
            ////nums.Insert(76);
            ////nums.Insert(72);
            //Console.WriteLine("\n----------Inorder---------\n");
            //Inorder(root);
            //Console.WriteLine("\n----------PreOrder---------\n");
            //PreOrder(root);
            //Console.WriteLine("\n----------PostOrder---------\n");
            //PostOrder(root);

            //Console.WriteLine(getHeight(root));
            //Console.WriteLine(height(root));
            //Console.ReadLine();
            //int[] param = new int[900000];

            //Random randNum = new Random();
            //for (int i = 0; i < param.Length; i++)
            //{
            //    param[i] = randNum.Next(1, 7);
            //}

            //string errorJson = JsonConvert.SerializeObject(param);
            //Console.WriteLine(errorJson);

            ////var beginer = missingNumber(param);

            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //Console.WriteLine(solution(param));
            //watch.Stop();
            //var elapsedMs = watch.ElapsedMilliseconds;
            //Console.WriteLine(elapsedMs);

            //Console.WriteLine("\n");
            //var watchB = System.Diagnostics.Stopwatch.StartNew();
            //Console.WriteLine(missingNumber(param));
            //watchB.Stop();
            //var elapsedMsB = watchB.ElapsedMilliseconds;
            //Console.WriteLine(elapsedMsB);








            //var rollWatch = System.Diagnostics.Stopwatch.StartNew();
            //Console.WriteLine(rollDice(param));
            //watch.Stop();
            //var rollElapsed = rollWatch.ElapsedMilliseconds;
            //Console.WriteLine(rollElapsed);

            //var rounds = minimumRounds(129, 2);
            //long[] A = { 0, 0, 0, 0,0,0,0,0,0,0 };
            //long n = A.Length;
            //// Create and fill difference Array 
            //// We use one extra space because 
            //// update(l, r, x) updates D[r+1] 
            //long[] D = new long[n + 1];
            //initializeDiffArray(A, D);

            //// After below update(l, r, x), the 
            //// elements should become 20, 15, 20, 40 
            //update(D, 2, 6, 8);
            //update(D, 3, 5, 7);
            //update(D, 1, 8, 1);
            //update(D, 5, 9, 15);

            //var maxSUm = printArray(A, D);

            //var ans1 = getIdealNums(1, 1);

            //List<List<string>> myList = new List<List<string>>();
            //myList.Add(new List<string> { "a", "b" });
            //myList.Add(new List<string> { "c", "d", "e" });
            //myList.Add(new List<string> { "qwerty", "asdf", "zxcv" });
            //myList.Add(new List<string> { "a", "b" });

            //var ans = multiple(3, 5, 10, 12);

            //int[,] a = new int[3, 4] {
            //   {1, 1, 1, 1} ,   /*  initializers for row indexed by 0 */
            //   {1, 1, 1, 1} ,   /*  initializers for row indexed by 1 */
            //   {1, 1, 1, 1}   /*  initializers for row indexed by 2 */
            //};

            //var res = NumOffices(a, 3, 4);

            ////int[,] b = new int[3, 4];

            ////Array.Copy(a, b, 4);

            ////var days = minimumDays(3, 4, a);

            //var toys = new List<string> { "elmo", "elsa", "legos", "drone", "tablet", "warcraft" };
            //var quotes = new List<string> {
            //    "elmo is the hottest",
            //    "the new Elmo dolls are super high quality",
            //    "expect the elsa dolls to be very",
            //    "elsa and Elmo are the toys",
            //    "For parents older kids, look into buying them a drone",
            //    "warcraft is slowly rising in pop" };
            ////var res = popularNToys(6, 2, toys, 6, quotes);

            //Console.ReadLine();

            ////var arr = new int[] { 2,4,6,8,10 };
            ////var res = generalizedGCD(5, arr);


            ////int time = 0;

            ////List<LogOutput> logs = new List<LogOutput>();
            ////bool shouldContinue = true;
            ////bool firstLineCollected = false;
            ////while (shouldContinue)
            ////{
            ////    var input = Console.ReadLine();
            ////    if (!string.IsNullOrEmpty(input))
            ////    {
            ////        if (!firstLineCollected)
            ////        {
            ////            time = Convert.ToInt32(input);
            ////            firstLineCollected = true;

            ////        }
            ////        else
            ////        {
            ////            string[] output = new string[3];
            ////            var log = new LogOutput();
            ////            output = Array.ConvertAll(input.Split(' '), arrTemp => Convert.ToString(arrTemp));

            ////            log.Time = Convert.ToInt32(output[0]);
            ////            log.ProcessName = (output[1]);
            ////            log.Status = (output[2]);
            ////            logs.Add(log);
            ////        }
            ////    }
            ////    else
            ////    {
            ////        shouldContinue = false;
            ////    }
            ////}

            ////var foundLog = new List<LogOutput>();
            ////for(var i = 0; i < logs.Count; i++)
            ////{
            ////    if(logs[i].Time == time && logs[i].Status== "running")
            ////    {
            ////        foundLog.Add(logs[i]);
            ////    }
            ////}


            ////if(foundLog.Count == 1)
            ////{
            ////    Console.WriteLine(foundLog[0].ProcessName);
            ////}
            ////else
            ////{
            ////    Console.WriteLine(-1);
            ////}

            Console.ReadLine();
        }

        public static void solution(int[] arrs, string[] ops)
        {
            int ans = 0;
            foreach (int num in arrs)
            {
                int acc = num;
                for (var i = 0; i < ops.Length; i++)
                {
                    if (ops[i] == "root")
                    {
                        acc = Convert.ToInt32(Math.Floor(Math.Sqrt(acc)));
                    }
                    else if (ops[i] == "square")
                    {
                        acc = Convert.ToInt32(Math.Pow(acc, 2));
                    }
                    else if (ops[i] == "accumulate")
                    {
                        ans += acc;
                    }
                }
                Console.WriteLine(ans);
            }
        }

        public static int ParseToInt(string value)
        {
            int result = 0;
            int offset = 48; // ascii 48 = zero

            for (int index = 0; index < value.Length; index++)
            {
                char letter = value[index];
                if (letter < 48 || letter > 57)
                    throw new FormatException("Unable to parse value: " + value);
                //var first = 
                result = 10 * result + (letter - offset); 
            }
            return result;
        }

        static bool IsPrime(int n)
        {
            // Corner cases 
            if (n <= 1)
                return false;
            if (n <= 3)
                return true;

            // This is checked so that we can skip 
            // middle five numbers in below loop 
            if (n % 2 == 0 || n % 3 == 0)
                return false;

            for (int i = 5; i * i <= n; i += 6)
                if (n % i == 0 || n % (i + 2) == 0)
                    return false;

            return true;
        }

        static void IsPrime()
        {
            int num, i, ctr, stno, enno;

            Console.Write("\n\n");
            Console.Write("Find the prime numbers within a range of numbers:\n");
            Console.Write("---------------------------------------------------");
            Console.Write("\n\n");

            Console.Write("Input starting number of range: ");
            stno = Convert.ToInt32(Console.ReadLine());
            Console.Write("Input ending number of range : ");
            enno = Convert.ToInt32(Console.ReadLine());
            Console.Write("The prime numbers between {0} and {1} are : \n", stno, enno);

            for (num = stno; num <= enno; num++)
            {
                if (IsPrime(num))
                    Console.Write(num + " ");
            }
            Console.Write("\n");
        }

        public static int missingNumber(int[] nums)
        {
            // initializations
            int min = int.MaxValue;
            int temp;
            int[] count = new int[7];

            // counting occurrences of each number in the nums array and placing in count[]
            foreach (int num in nums) count[num]++;

            // can flip each dice to any number between 1 and 6, so we find the min of each possible top face.
            for (int i = 1; i < 7; i++)
            {
                /*
                 * count twice if compliment of desired (2*count[7-desired]) +
                 * total number of dice we have (nums.length) -
                 * count of desired occurrences (count[desired] -
                 * count of compliments (count[7-desired]).
                 * simplify to:
                 */
                temp = 2 * count[7 - i] + nums.Length - count[i] - count[7 - i];
                // check if what we calculated for moves is less than something we already found.
                min = temp < min ? temp : min;
            }
            return min;
        }

        public static int diceHelper(int source, int target)
        {
            if (source == target) return 0;
            else if (source + target == 7) return 2;
            else return 1;
        }

        public static int rollDice(int[] dice)
        {
            int rolls = int.MaxValue;

            for (int i = 0; i < dice.Length; ++i)
            {
                int tmp = 0;
                for (int j = 0; j < dice.Length; ++j)
                    tmp += diceHelper(dice[i], dice[j]);
                if (tmp < rolls)
                    rolls = tmp;
            }
            return rolls;
        }

        public static int solutionB(int[] dices)
        {
            int res = dices.Length * 2;
            int[] count = new int[6];
            foreach (int dice in dices)
            {
                count[dice - 1]++;
            }
            for (int i = 0; i < 6; i++)
            {
                res = Math.Min(res, count[5 - i] + dices.Length - count[i]);
            }
            return res;
        }

        static int minimumRounds(int N, int K)
        {
            int rounds = 0;
            if (K == 0)
            {
                return N - 1;
            }
            while (K > 0 && N > 0)
            {
                if (N == 2)
                {
                    rounds++;
                    N = 0;
                }

                if (N % 2 == 0 && N > 0)
                {
                    N = N / 2;
                    K -= 1;
                    rounds++;
                }
                else if ((N - 1) % 2 == 0 && N > 0)
                {
                    N -= 1;
                    N = N / 2;
                    K -= 1;
                    rounds += 2;
                }
                else if (N > 0)
                {
                    N -= 1;
                    rounds++;
                }
            }


            if (K == 0 && N > 0)
            {
                rounds += N - 1;
            }

            return rounds;
        }
        static int missingPerm(int[] A)
        {

            var workingArr = A.Distinct().ToArray();
            if (workingArr.Length != A.Length) return 0;

            Array.Sort(A);

            var arrLen = A.Length;
            int lastItem = A[arrLen - 1];
            var actualSum = (arrLen * (arrLen + 1)) / 2;
            var expected = (lastItem * (lastItem + 1)) / 2;

            if (expected != actualSum) return 0;
            else return 1;
        }

        //Complete the arrayManipulation function below.

        //Creates a diff array D[] for A[] and returns
        // it after filling initial values.
        static void initializeDiffArray(long[] A, long[] D)
        {

            int n = A.Length;

            D[0] = A[0];
            D[n] = 0;
            for (int i = 1; i < n; i++)
                D[i] = A[i] - A[i - 1];
        }

        // Does range update 
        static void update(long[] D, long l, long r, long x)
        {
            D[l] += x;
            D[r + 1] -= x;
        }

        // Prints updated Array 
        static long printArray(long[] A, long[] D)
        {
            var max = long.MinValue;
            for (int i = 0; i < A.Length; i++)
            {

                if (i == 0)
                {

                    A[i] = D[i];
                    max = Math.Max(max, A[i]);
                }

                // Note that A[0] or D[0] decides 
                // values of rest of the elements. 
                else
                {
                    A[i] = D[i] + A[i - 1];
                    max = Math.Max(max, A[i]);
                }

                Console.Write(A[i] + " ");
            }

            Console.WriteLine();

            return max;
        }

        static long arrayManipulation(int n, int[][] queries)
        {

            long[] arr = new long[n];
            long[] diff = new long[n + 1];

            long max = long.MinValue;
            for (var i = 0; i < queries.Length; i++)
            {
                var a = queries[i][0] - 1;
                var b = queries[i][1] - 1;
                var k = queries[i][2];

                update(diff, a, b, k);
            }
            return printArray(arr, diff);
        }

        private static List<int[]> FindPairs(IEnumerable<int> sequence, int threshold)
        {
            var sorted = sequence.OrderBy(x => x).ToList();

            var pairs = new List<int[]>();
            for (var i = 0; i < sorted.Count - 1; ++i)
            {
                if (sorted[i + 2] - sorted[i] <= threshold)
                {
                    pairs.Add(new[] { sorted[i], sorted[i + 2] });
                }
            }

            return pairs;
        }

        static int minUniqueSum(List<int> arr)
        {
            int n = arr.Count();

            int sum = arr[0];
            int prev = arr[0];

            for (int i = 1; i < n; i++)
            {
                int curr = arr[i];

                if (prev >= curr)
                {
                    curr = prev + 1;
                }
                sum += curr;
                prev = curr;
            }

            return sum;
        }

        public static List<int[]> FindPossibleSeats(int len)
        {
            var pairs = new List<int[]>();
            for (var i = 1; i < len; i++)
            {
                if (i % 2 == 0)
                {
                    if (i + 2 <= len)
                    {
                        pairs.Add(new[] { i, i + 2 });

                    }
                }
                else if (i % 2 == 1)
                {
                    if (i + 1 <= len)
                    {
                        pairs.Add(new[] { i, i + 1 });
                    }
                    if (i + 2 <= len)
                    {
                        pairs.Add(new[] { i, i + 2 });
                    }

                }
            }

            return pairs;
        }

        static int getNumber(int[] arr, int arrLen)
        {
            var grouped = arr
                    .GroupBy(b => b)
                    .Select(g => new { Value = g.Key, Count = g.Count() })
                    .OrderByDescending(c => c.Count)
                    .ThenBy(c => c.Value)
                    .FirstOrDefault();
            return grouped.Value;
        }

        static int getUnpairdGuy(int[] A)
        {
            var grouped = A
                    .GroupBy(b => b)
                    .Select(g => new { Value = g.Key, Count = g.Count() })
                    .Where(g => g.Count % 2 != 0)
                    .FirstOrDefault();
            return grouped.Value;
        }

        //static void Main(string[] args)
        //{
        //    int[][] arr = new int[6][];

        //    for (int i = 0; i < 6; i++)
        //    {
        //        arr[i] = Array.ConvertAll(Console.ReadLine().Split(' '), arrTemp => Convert.ToInt32(arrTemp));
        //    }
        //}

        // Complete the matchingStrings function below.
        static int[] matchingStrings(string[] strings, string[] queries)
        {
            int queriesLength = queries.Length;
            var result = new int[queriesLength];
            for (var i = 0; i < queriesLength; i++)
            {
                result[i] = strings.Where(s => s == queries[i]).Count();
            }
            return result;
        }

        static int maxHourGlass(int[][] arr)
        {
            int max = int.MinValue;

            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    int sum = arr[i][j] + arr[i][j + 1] + arr[i][j + 2] + arr[i + 1][j + 1] + arr[i + 2][j] + arr[i + 2][j + 1] + arr[i + 2][j + 2];
                    max = Math.Max(max, sum);
                }
            }

            return max;
        }

        public static int minimumSteps(string[] steps)
        {
            var moves = 0;

            foreach (var op in steps)
            {
                if (op == "../")
                {
                    moves--;
                }
                else if (op == "./")
                {
                    continue;
                }
                else
                {
                    moves++;
                }
            }

            return moves;
        }

        public void bubbleSort(int[] a)
        {
            int n = a.Length;
            int numberOfSwaps = 0;
            for (int i = 0; i < n; i++)
            {
                // Track number of elements swapped during a single array traversal
                for (int j = 0; j < n - 1; j++)
                {
                    // Swap adjacent elements if they are in decreasing order
                    if (a[j] > a[j + 1])
                    {
                        swap(a, j, j + 1);
                        numberOfSwaps++;
                    }
                }

                // If no elements were swapped during a traversal, array is sorted
                if (numberOfSwaps == 0)
                {
                    break;
                }
            }
        }

        static int[] swap(int[] arr, int highIndex, int lowIndex)
        {
            int low = arr[lowIndex];
            arr[lowIndex] = arr[highIndex];
            arr[highIndex] = low;

            return arr;
        }

        public static string getMaxRange(string input)
        {
            Console.WriteLine(Char.GetNumericValue(input[0]));
            if (Char.GetNumericValue(input[0]) < 9)
            {
                Console.WriteLine(input[0]);
                return input.Replace(input[0], '9');
            }
            for (var i = 1; i < input.Length; i++)
            {
                Console.WriteLine(Char.GetNumericValue(input[i]));
                if (Char.GetNumericValue(input[i]) < 9)
                {
                    return input.Replace(input[i], '9');
                }
            }
            var firstChar = input[0];
            return input.Replace(firstChar, (char)(firstChar - 1));
        }

        public static int NumOffices(char[][] grid)
        {
            bool[,] visited = new bool[grid.Length, grid[0].Length];
            int res = 0;
            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid[0].Length; j++)
                {
                    if (grid[i][j] == '1' && !visited[i, j])
                    {
                        Search(grid, visited, i, j);
                        res++;
                    }
                }
            }
            return res;
        }

        public static long getIdealNums(long low, long high)
        {
            long count = 0;
            var threeArr = new double[9];
            var fiveArr = new double[9];
            for (var i = 0; i < 9; i++)
            {
                var three = Math.Pow(3, i);
                var five = Math.Pow(5, i);

                threeArr[i] = three;
                fiveArr[i] = five;
            }

            for (var i = 0; i < 9; i++)
            {

                for (var j = 0; j < 9; j++)
                {
                    var prod = Convert.ToDouble(threeArr[i] * fiveArr[j]);
                    if (prod >= low && prod <= high)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static int minimumSwaps(List<int> arr)
        {
            var swap = 0;
            for (var i = 0; i < arr.Count; i++)
            {
                while (i + 1 != arr[i])
                {
                    var temp = arr[arr[i] - 1];
                    arr[arr[i] - 1] = arr[i];
                    arr[i] = temp;
                    swap += 1;
                }
            }
            return swap;
        }

        public static List<int> multiple(int x, int y, int z, int n)
        {
            List<int> ans = new List<int>();
            var min = Math.Min(x, y);
            for (var i = min; i <= n; i++)
            {
                if (x != 0 && y != 0 && z != 0 && i % z != 0 && (i % x == 0 || i % y == 0))
                {
                    ans.Add(i);
                }
            }
            return ans;
        }

        public static void Search(char[][] grid, bool[,] visited, int i, int j)
        {
            if (i < 0 || i >= grid.Length) return;
            if (j < 0 || j >= grid[0].Length) return;
            if (grid[i][j] != '1' || visited[i, j]) return;
            visited[i, j] = true;
            Search(grid, visited, i + 1, j);
            Search(grid, visited, i - 1, j);
            Search(grid, visited, i, j + 1);
            Search(grid, visited, i, j - 1);
        }

        //public static List<List<int>> employeeWithLesserThanKBreaks(List<List<int>> employeeCalls, int k)
        //{

        //    var points = new HashSet<(int x, int y)>();
        //    var groupedCalls = employeeCalls.GroupBy(g => g[0])
        //                        .Select(grp => new 
        //                        {
        //                            Id = grp.Key,
        //                            Percentage = grp.ToList()
        //                        }).ToList();
        //    List<List<int>> myList = new List<List<int>>();

        //    foreach (var item in groupedCalls)
        //    {
        //        if (item.Percentage.Count > k)
        //        {
        //            for (var i = 0; i < item.Percentage.Count - 1; i++)
        //            {
        //                if (item.Percentage[i][2] != item.Percentage[i+1][1])
        //                {
        //                    if(points.Contains(item.Percentage[i][0],item[]))
        //                    myList.Add(new List<int> {item.Percentage[i][0], })
        //                }
        //            }

        //        }

        //    }
        //    return groupedCalls;
        //}

        public static List<int> missingReservations(List<List<int>> firstReservationList, List<List<int>> secondReservationList)
        {
            var notExisting = secondReservationList.Where(des => firstReservationList.All(src => src[0] != des[0])).Select(a => a[0]).ToList();
            return notExisting;
        }

        public class LogOutput
        {
            public int Time { get; set; }
            public string ProcessName { get; set; }
            public string Status { get; set; }
        }

        //static Node insert(Node head, int data)
        //{
        //    Node p = new Node(data);

        //    if (head == null)
        //        head = p;
        //    else if (head.right == null)
        //        head.right = p;
        //    else
        //    {
        //        Node start = head;
        //        while (start.right != null)
        //            start = start.right;
        //        start.right = p;

        //    }
        //    return head;
        //}

        static int countItem(int[] arr, int A)
        {
            return arr.Where(i => i == A).Count();
        }

        static double getDistance(int[] a, int[] b, int[] c)
        {
            var b0a0 = b[0] - a[0];
            var b1a1 = b[1] - a[1];

            var c0b0 = c[0] - b[0];
            var c1b1 = c[1] - b[1];

            var a0c0 = a[0] - c[0];
            var a1c1 = a[1] - c[1];

            var d1 = Math.Sqrt((Math.Pow(b0a0, 2)) + (Math.Pow(b1a1, 2)));
            var d2 = Math.Sqrt((Math.Pow(c0b0, 2)) + (Math.Pow(c1b1, 2)));
            var d3 = Math.Sqrt((Math.Pow(a0c0, 2)) + (Math.Pow(a1c1, 2)));

            var totalDistance = d1 + d2 + d3;
            return totalDistance / 3;

        }

        static Node removeDuplicates(Node head)
        {
            if (head != null)
            {
                var lastNode = head;
                var currentHead = head.right;
                while (currentHead != null)
                {
                    if (currentHead.data != lastNode.data)
                    {
                        lastNode.right = currentHead;
                        lastNode = currentHead;
                    }


                    currentHead = currentHead.right;

                }
                //lastNode.right = null;
                return head;
            }

            return null;

        }

        class Node
        {
            public Node left, right;
            public int data;
            public Node(int data)
            {
                this.data = data;
                left = right = null;
            }
        }

        static Node insert(Node root, int data)
        {
            if (root == null)
            {
                root = new Node(data);
                return root;
            }
            else
            {
                Node cur;
                if (data <= root.data)
                {
                    cur = insert(root.left, data);
                    root.left = cur;
                }
                else
                {
                    cur = insert(root.right, data);
                    root.right = cur;
                }
                return root;
            }
        }

        public class BinarySearchTree
        {
            public class Node
            {
                public Node right, left;
                public int data;
                public Node(int data)
                {
                    this.data = data;
                    right = left = null;
                }
            }

            public Node Insert(int data, Node root)
            {
                if (root == null)
                {
                    root = new Node(data);
                    return root;
                }
                else
                {
                    if (data <= root.data)
                    {
                        root.left = Insert(data, root.left);
                    }
                    else
                    {
                        root.right = Insert(data, root.right);
                    }
                }
                return root;
            }

            public void Inorder(Node node)
            {
                if (node != null)
                {
                    Inorder(node.left);
                    Console.Write(node.data + " ");
                    Inorder(node.right);
                }
                //Console.WriteLine("----------InOrder---------");
            }

            public void PreOrder(Node node)
            {
                if (node != null)
                {
                    Console.Write(node.data + " ");
                    PreOrder(node.left);
                    PreOrder(node.right);
                }
            }

            public void PostOrder(Node node)
            {
                if (node != null)
                {
                    PostOrder(node.left);
                    PostOrder(node.right);
                    Console.Write(node.data + " ");
                }
                //Console.WriteLine("----------PostOrder---------");
            }

            static int height(Node root)
            {
                if (root == null)
                {
                    return 0;
                }
                else
                {
                    /* compute height of each subtree */
                    int lheight = height(root.left);
                    int rheight = height(root.right);

                    /* use the larger one */
                    if (lheight > rheight)
                    {
                        return (lheight + 1);
                    }
                    else
                    {
                        return (rheight + 1);
                    }
                }
            }

            static int getHeight(Node root)
            {
                //Write your code here
                if (root == null)
                {
                    return -1;
                }
                else if (root.left == null && root.right == null)
                {
                    return 0;
                }
                int rightHeight = getHeight(root.right);
                int leftHeight = getHeight(root.left);
                return Math.Max(rightHeight, leftHeight) + 1;
            }

            static void levelOrder(Node root)
            {
                //Write your code here
                Queue<Node> queue = new Queue<Node>();

                queue.Enqueue(root);
                while (queue.Count() > 0)
                {
                    var current = queue.Dequeue();
                    Console.Write(current.data + " ");
                    if (current.left != null)
                    {
                        queue.Enqueue(current.left);
                    }
                    if (current.right != null)
                    {
                        queue.Enqueue(current.right);
                    }
                }
            }
        }

        public static string getMinRange(string input)
        {
            if (Char.GetNumericValue(input[0]) > 1)
            {
                return input.Replace(input[0], '1');
            }

            for (var i = 1; i < input.Length; i++)
            {
                if (Char.GetNumericValue(input[i]) > 0)
                {
                    var charToReplace = input[i];
                    return input.Replace(charToReplace, '0');
                }
            }

            input = input.Replace(input[1], '1');

            return input;
        }

        public static int minimumDays(int rows, int columns, int[,] grid)
        {
            int days = 0;
            var hasZeroes = true;
            while (hasZeroes)
            {
                var changed = false;
                var points = new HashSet<(int x, int y)>();
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        if (grid[i, j] == 0)
                        {
                            if (j != 0 && !points.Contains((i, j - 1)) && grid[i, j - 1] == 1)
                            {
                                changed = true;
                                grid[i, j] = 1;
                                points.Add((i, j));
                                continue;
                            }

                            if (i != 0 && !points.Contains((i - 1, j)) && grid[i - 1, j] == 1)
                            {
                                changed = true;
                                grid[i, j] = 1;
                                points.Add((i, j));
                                continue;
                            }

                            if (j != columns - 1 && !points.Contains((i, j + 1)) && grid[i, j + 1] == 1)
                            {
                                changed = true;
                                grid[i, j] = 1;
                                points.Add((i, j));
                                continue;
                            }

                            if (i != rows - 1 && !points.Contains((i + 1, j)) && grid[i + 1, j] == 1)
                            {
                                changed = true;
                                grid[i, j] = 1;
                                points.Add((i, j));
                                continue;
                            }
                        }
                    }
                }

                hasZeroes = changed;
                days++;
            }

            return days - 1;
        }

        public static List<string> popularNToys(int numToys, int topToys, List<string> toys, int numQuotes, List<string> quotes)
        {
            var toyResult = new List<string>();
            if (topToys > numToys)
            {
                foreach (var toy in toys)
                {
                    foreach (var quote in quotes)
                    {
                        if (quote.Contains(toy, StringComparison.OrdinalIgnoreCase))
                        {
                            toyResult.Add(toy);
                        }
                    }
                }
                return toyResult;
            }

            var dictionary = new Dictionary<string, int>();

            foreach (var toy in toys)
            {
                var count = quotes.Where(x => x.Contains(toy, StringComparison.OrdinalIgnoreCase)).Count();

                //var count = quotes
                //    .Count(request => request.Contains(toy, StringComparison.OrdinalIgnoreCase));
                dictionary.TryAdd(toy, count);
            }
            var result = dictionary.OrderByDescending(item => item.Value).ThenBy(item => item.Key).Take(topToys).Select(item => item.Key).ToList();
            return result;
        }

        public static List<string> popularNFeatures(int numFeatures, int topFeatures, List<string> possibleFeatures, int numFeatureRequests, List<string> featureRequests)
        {
            if (topFeatures > numFeatureRequests)
            {
                var list = new List<string>();
                possibleFeatures.ForEach(feature =>
                {
                    if (featureRequests.Any(request => request.Contains(feature, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(feature);
                    }
                });
                return list;
            }

            var dictionary = new Dictionary<string, int>();
            foreach (var feature in possibleFeatures)
            {
                var count = featureRequests
                    .Count(request => request.Contains(feature, StringComparison.OrdinalIgnoreCase));
                dictionary.TryAdd(feature, count);
            }
            var result = dictionary.OrderByDescending(item => item.Value).ThenBy(item => item.Key).Take(topFeatures).Select(item => item.Key).ToList();
            return result;
            // WRITE YOUR CODE HERE
        }

        public static string Encrypt(string key, string toEncrypt, bool useHashing = true)
        {
            byte[] resultArray = null;
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);

                if (useHashing)
                {
                    using (MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider())
                    {
                        keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));
                    }
                }
                else
                    keyArray = Encoding.UTF8.GetBytes(key);


                using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = keyArray;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cTransform = tdes.CreateEncryptor();
                    resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string key, string cipherString, bool useHashing = true)
        {
            byte[] resultArray = null;
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = Convert.FromBase64String(cipherString);

                if (useHashing)
                {
                    using (MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider())
                    {
                        keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    }
                }
                else
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);


                using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = keyArray;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform cTransform = tdes.CreateDecryptor();
                    resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        static int binarySearch(int[] arr, int low, int high, int x)
        {
            if (high >= low)
            {
                int mid = low + (high - low) / 2;
                if (x == arr[mid])
                    return mid;
                if (x > arr[mid])
                    return binarySearch(arr, (mid + 1),
                                              high, x);
                else
                    return binarySearch(arr, low,
                                         (mid - 1), x);
            }

            return -1;
        }

        static int countPairsWithDiffK(int[] arr, int n, int k)
        {

            int count = 0, i;

            // Sort array elements 
            Array.Sort(arr);

            for (i = 0; i < n - 1; i++)
                if (binarySearch(arr, i + 1, n - 1,
                                arr[i] + k) != -1)
                    count++;

            return count;
        }

        public static int NumOffices(int[,] grid, int row, int column)
        {
            bool[,] visited = new bool[row, column];
            int res = 0;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    if (grid[i, j] == 1 && !visited[i, j])
                    {
                        search(grid, visited, i, j, row, column);
                        res++;
                    }
                }
            }
            return res;
        }

        public static void search(int[,] grid, bool[,] visited, int i, int j, int row, int column)
        {
            if (i < 0 || i >= row) return;
            if (j < 0 || j >= column) return;
            if (grid[i, j] != 1 || visited[i, j]) return;
            visited[i, j] = true;
            search(grid, visited, i + 1, j, row, column);
            search(grid, visited, i - 1, j, row, column);
            search(grid, visited, i, j + 1, row, column);
            search(grid, visited, i, j - 1, row, column);
        }

        //static void Main(string[] args)
        //{
        //    Console.WriteLine("Hello World!");

        //    Console.WriteLine(DateTime.Now.ToString());

        //    var arr = new[] { 8,1,8 };

        //    var classLen = arr[0];
        //    var occupied = arr.Skip(1).Take(arr.Length - 1);
        //    var classDesks = new int[classLen];
        //    for (int i = 0; i < classLen; i++)
        //    {
        //        classDesks[i] = i + 1;
        //    }

        //    var unoccupied = classDesks.Except(occupied);
        //    var res = FindPossibleSeats(classLen);

        //    var count = 0;
        //    foreach(var iten in res)
        //    {
        //        if(!occupied.Contains(iten[0]) && !occupied.Contains(iten[1]))
        //        {
        //            count++;
        //        }
        //    }

        //    Console.WriteLine(count);

        //    //int l = Convert.ToInt32(Console.ReadLine().Trim());
        //    //var ballot = new List<string>();
        //    //while (l > 0)
        //    //{
        //    //    var name = Console.ReadLine();
        //    //    ballot.Add(name);
        //    //    l--;


        //    //}
        //    //writeln(ballot);

        //    //int r = Convert.ToInt32(Console.ReadLine().Trim());

        //    //List<int> res = oddNumbers(l, r);
        //    var test = new string[3];
        //    test[0] = "{(([])[])[]}";
        //    test[1] = "{[(])}";
        //    test[2] = "{{[[(())]]}}";



        //    Console.WriteLine(isBalanced(test));


        //    ///int result = check(new int[] { 1, 3, 1, 4, 2, 3, 5, 4 }, 5);
        //    //string test = "1 2 3 4 5 6";
        //    //var arr = string.Join(' ', test).ToArray();

        //    //var sec = test.Split(" ");

        //    //string line;
        //    //var lines = new List<String>();
        //    //while ((line = Console.ReadLine()) != "")
        //    //{
        //    //    lines.Add(line);
        //    //    //Console.WriteLine(line);
        //    //}
        //    //var numberList = lines[1].Split(' ');

        //    //int degree = 1, candidateDegree = 1;

        //    //for (int i = 0; i < numberList.Length; i++)
        //    //{
        //    //    if (i > 0 && numberList[i] == numberList[i - 1])
        //    //    {
        //    //        degree += 1;
        //    //    }
        //    //    else
        //    //    {
        //    //        candidateDegree = degree > candidateDegree ? degree : candidateDegree;
        //    //        degree = 1;
        //    //    }
        //    //}

        //    //Console.WriteLine(candidateDegree);

        //    //int[] test2 = new int[5];

        //    //Random randNum = new Random();
        //    //for (int i = 0; i < test2.Length; i++)
        //    //{
        //    //    test2[i] = randNum.Next(1, 7);
        //    //}
        //    //var param = new int[] { 2,2,2,2,5 };

        //    // Console.WriteLine(string.Join(' ', test2));
        //    //// Console.WriteLine(OneDoesntHaveMirrorImage(test2));
        //    //var watch = System.Diagnostics.Stopwatch.StartNew();
        //    //Console.WriteLine(solution(param));
        //    //watch.Stop();
        //    //var elapsedMs = watch.ElapsedMilliseconds;
        //    //Console.WriteLine(elapsedMs);
        //    //Console.ReadLine();
        //}

        static List<int> oddNumbers(int l, int r)
        {
            List<int> ans = new List<int>();
            while (r + 1 > l)
            {
                if (l % 2 == 1)
                {
                    ans.Add(l);
                }
                l++;
            }

            var indexO = ans.IndexOf(4);
            return ans;
        }

        //var laxy = test.Split(' ').Select(n => Convert.ToInt32(n)).ToArray();

        public static bool OneDoesntHaveMirrorImage(int[] A)
        {
            for (var i = 0; i < A.Length; i++)
            {
                var foos = new List<int>(A);
                foos.RemoveAt(i);
                var restOfArray = foos.ToArray();

                if ((A[i] == 3 && Array.IndexOf(restOfArray, 4) < 0) || (A[i] == 4 && Array.IndexOf(restOfArray, 3) < 0))
                {
                    return true;
                }
                else if ((A[i] == 2 && Array.IndexOf(restOfArray, 5) < 0) || (A[i] == 5 && Array.IndexOf(restOfArray, 2) < 0))
                {
                    return true;
                }
                else if ((A[i] == 1 && Array.IndexOf(restOfArray, 6) < 0) || (A[i] == 6 && Array.IndexOf(restOfArray, 1) < 0))
                {
                    return true;
                }
            }
            return false;
        }

        public static int getCountWithDuplicateEntries(int[] query, int size)
        {
            var leastSum = size;
            for (var i = 1; i < 7; i++)
            {
                int mirroImage = query[7 - i];

                int sum = size + mirroImage - query[i];
                leastSum = Math.Min(sum, leastSum);
            }
            return leastSum;
        }

        public static string writeln(List<string> ballot)
        {

            var grouped = ballot
                .GroupBy(b => b)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .ThenByDescending(n => n.Name).First().Name;
            return "YES";
        }

        static string[] isBalanced(string[] values)
        {

            string[] status = new string[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                //if length of item is not even, no need to proceed
                if (values[i].Length % 2 != 0)
                {
                    status[i] = "NO";
                    continue;
                }
                char c;
                Stack<char> stack = new Stack<char>();
                string stat = "YES";
                for (var j = 0; j < values[i].Length; j++)
                {
                    if (stack.Count == 0)
                    {
                        if (values[i][j] == ')' || values[i][j] == '}' || values[i][j] == ']')
                        {
                            stat = "NO";
                            //stack.Pop();
                            break;
                        }
                        stack.Push(values[i][j]);
                        continue;
                    }

                    c = stack.Peek();
                    switch (values[i][j])
                    {
                        case '}':
                        case ']':
                            if (c != values[i][j] - 2)
                            {
                                stat = "NO";
                                break;
                            }
                            stack.Pop();
                            break;
                        case ')':
                            if (c != '(')
                            {
                                stat = "NO";
                                break;
                            }
                            stack.Pop();
                            break;
                        default:
                            stack.Push(values[i][j]);
                            break;
                    }
                }

                status[i] = stat;
            }

            return status;

            //// Complete this function

        }

        public static string getStatus(string s)
        {
            if (s.Length % 2 == 1) return "NO";

            Stack<char> sta = new Stack<char>();
            char c;
            for (int i = 0; i < s.Length; i++)
            {
                if (sta.Count == 0)
                {
                    if (s[i] == ')' || s[i] == ']' || s[i] == '}') return "NO";
                    sta.Push(s[i]);
                    continue;
                }
                c = sta.Peek();
                switch (s[i])
                {
                    case '}':
                    case ']':
                        if (c != s[i] - 2) return "NO";
                        sta.Pop();
                        break;
                    case ')':
                        if (c != '(') return "NO";
                        sta.Pop();
                        break;
                    default: sta.Push(s[i]); break;
                }
            }
            return (sta.Count > 0) ? "NO" : "YES";
        }

        public static int solution(int[] A)
        {
            int[] arr = new int[7];

            foreach (var item in A)
            {
                arr[item]++;
            }

            int size = A.Length;
            var count = 0;

            count = getCountWithDuplicateEntries(arr, size);

            return count;
        }

        public class Counter
        {
            public int Value { get; set; }
            public int Count { get; set; }
            public int Sum { get; set; }
        }

        public static int[] cellCompete(int[] states, int days)
        {
            int[] newArr = new int[states.Length];

            while (days > 0)
            {
                for (var i = 0; i < states.Length; i++)
                {
                    int leftIndex = i - 1;
                    int rightIndex = i + 1;

                    int leftCell = leftIndex < 0 ? 0 : states[leftIndex];
                    int rightCell = rightIndex >= states.Length ? 0 : states[rightIndex];

                    int result = rightCell == leftCell ? 0 : 1;

                    newArr[i] = result;
                }



                Array.Copy(newArr, states, states.Length);
                days--;
            }
            return newArr;
        }

        public static int generalizedGCD(int num, int[] arr)
        {
            // WRITE YOUR CODE HERE
            Array.Sort(arr);

            int firstItem = arr[0];
            bool notFound = true;
            while (notFound)
            {
                for (int i = 0; i < num; i++)
                {
                    if (arr[i] % firstItem != 0)
                    {
                        notFound = true;
                        firstItem--;
                        continue;
                    }

                }
                notFound = false;

                return firstItem;
            }
            return firstItem;
        }
    }
}
