using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.AmazonPrep
{
    class BST
    {
        public class BinarySearchTree
        {

            public class Node
            {
                public int Data;
                public Node Left;
                public Node Right;

                public Node(int data)
                {
                    Data = data;
                    Left = null;
                    Right = null;
                }

                public void DisplayNode()
                {
                    Console.Write(Data + " ");
                }
            }

            public Node root;

            public BinarySearchTree()
            {
                root = null;
            }

            public void Insert(int i)
            {
                Node newNode = new Node(i);
                if (root == null)
                {
                    root = newNode;
                }
                else
                {
                    Node current = root;
                    Node parent;
                    while (true)
                    {
                        parent = current;
                        if (i < current.Data)
                        {
                            current = current.Left;
                            if (current == null)
                            {
                                parent.Left = newNode;
                                break;
                            }

                            else
                            {
                                current = current.Right;
                                if (current == null)
                                {
                                    parent.Right = newNode;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            public void Inorder(Node node)
            {
                if (node != null)
                {
                    Inorder(node.Left);
                    Console.WriteLine(node.Data);
                    Inorder(node.Right);
                }
            }

            public void PreOrder(Node node)
            {
                if (node != null)
                {
                    Console.WriteLine(node.Data);
                    PreOrder(node.Left);
                    PreOrder(node.Right);
                }
            }

            //public Node InsertMe(int i,Node root)
            //{
            //    if (root == null)
            //        root = new Node(i);
            //    else
            //    {
            //        Node current;
            //        if (i<=root.Data)
            //        {
            //            current = in
            //        }
            //    }
            //}

            public void PostOrder(Node node)
            {
                if (node != null)
                {
                    PostOrder(node.Left);
                    PostOrder(node.Right);
                    Console.WriteLine(node.Data);
                }
            }

            //static void Main()
            //{
            //    BinarySearchTree nums = new BinarySearchTree();
            //    nums.Insert(50);
            //    nums.Insert(17);
            //    nums.Insert(23);
            //    nums.Insert(12);
            //    nums.Insert(19);
            //    nums.Insert(54);
            //    nums.Insert(9);
            //    nums.Insert(14);
            //    nums.Insert(67);
            //    nums.Insert(76);
            //    nums.Insert(72);
            //}
        }
    }
}
