using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Joe
{
    public class GameManager : MonoBehaviour
    {

        public int maxHeight = 15;
        public int maxWidth = 17;

        public Color color1;
        public Color color2;
        public Color appleColor;
        public Color playerColor = Color.black;

        public GameObject player;
        Node playerNode;

        public GameObject apple;
        Node appleNode;

        public GameObject tailParent;

        Node prevPlayerNode;



        public Transform cameraHolder;

        GameObject mapObject;
        SpriteRenderer sp;

        Node[,] grid;

        List<Node> availableNodes = new List<Node>();
        List<TailNode> tail = new List<TailNode>();


        bool up, left, right, down;

        public float MoveRate = 0.5f;
        float timer;

        Direction curDirection;

        public enum Direction
        {
            UP, DOWN, LEFT, RIGHT
        }

        #region Init
        // Use this for initialization
        void Start()
        {
            playerColor = Color.black;
            appleColor = Color.yellow;
            CreateMap();
            PlacePlayer();
            PlaceCamera();
            curDirection = Direction.RIGHT;
            CreateApple();
        }

        void CreateMap()
        {
            mapObject = new GameObject("map");
            sp = mapObject.AddComponent<SpriteRenderer>();

            grid = new Node[maxWidth, maxHeight];

            Texture2D txt = new Texture2D(maxWidth, maxHeight);
            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    Vector3 pos = Vector3.zero;
                    pos.x = x;
                    pos.y = y;


                    Node n = new Node() {
                        x = x,
                        y = y,
                        worldPosition = pos
                    };

                    grid[x, y] = n;
                    availableNodes.Add(n);


                    if (x % 2 != 0)
                    {
                        if (y % 2 != 0)
                        {
                            txt.SetPixel(x, y, color1);
                        }
                        else
                        {
                            txt.SetPixel(x, y, color2);
                        }
                    }
                    else
                    {
                        if (y % 2 != 0)
                        {
                            txt.SetPixel(x, y, color2);
                        }
                        else
                        {
                            txt.SetPixel(x, y, color1);
                        }
                    }

                }
            }
            txt.filterMode = FilterMode.Point;

            txt.Apply();
            Rect rect = new Rect(0, 0, maxWidth, maxHeight);
            Sprite sprite = Sprite.Create(txt, rect, Vector2.zero  , 1, 0, SpriteMeshType.FullRect);
            
            sp.sprite = sprite;
        }
        
        void PlacePlayer()
        {
            player = new GameObject("Player");
            SpriteRenderer playerSp = player.AddComponent<SpriteRenderer>();
            playerSp.sprite = CreateSprite(playerColor);
            playerSp.sortingOrder = 1;
            playerNode = GetNode(3, 3);

            PlacePlayerObject(player, playerNode.worldPosition);
            
            player.transform.localScale = Vector3.one * 1.2f;

            tailParent = new GameObject("TailParent");
            
        }

        void PlaceCamera()
        {
            Node n = GetNode(maxWidth / 2, maxHeight / 2);
            Vector3 p = n.worldPosition;

            p += Vector3.one * .5f; 

            cameraHolder.position = p;
        }

        void CreateApple()
        {
            apple = new GameObject("Apple");
            SpriteRenderer appleSp = apple.AddComponent<SpriteRenderer>();
            appleSp.sprite = CreateSprite(appleColor);
            appleSp.sortingOrder = 1;
            RandomApple();

        }
        #endregion


        #region Update
        void GetInput()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }

        private void Update()
        {
            GetInput();
            SetPlayerDirection();

            timer += Time.deltaTime;

            if(timer > MoveRate)
            {
                timer = 0;
                MovePlayer();
            }

            
        }

        void MovePlayer()
        {
            int x = 0;
            int y = 0;

            switch (curDirection)
            {
                case Direction.UP:
                    y = 1;
                    break;
                case Direction.DOWN:
                    y = -1;
                    break;
                case Direction.LEFT:
                    x = -1;
                    break;
                case Direction.RIGHT:
                    x = 1;
                    break;
            }
            Node targetNode = GetNode(playerNode.x + x, playerNode.y + y);

            if(targetNode == null)
            {
                //gameOver
            }
            else
            {
                if (isTailNode(targetNode))
                {
                    //gameOver
                }

                bool isScore = false;
                if(targetNode == appleNode)
                {
                    //score
                    isScore = true;
                    
                }

                Node previousNode = playerNode;

                availableNodes.Add(previousNode);

                
                if (isScore == true)
                {
                    tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                    availableNodes.Remove(previousNode);
                }

                MoveTail();

                PlacePlayerObject(player, targetNode.worldPosition);

                
                playerNode = targetNode;
                availableNodes.Remove(playerNode);

                if (isScore == true)
                {
                    if (availableNodes.Count > 0)
                    {
                        RandomApple();
                    }
                    else
                    {
                        //you win
                    }
                }
            }
        }

        void MoveTail()
        {
            Node previousNode = null;

            for (int i = 0; i < tail.Count; i++)
            {
                TailNode p = tail[i];
                availableNodes.Add(p.node);

                if(i == 0)
                {
                    previousNode = p.node;
                    p.node = playerNode;
                }
                else
                {
                    Node prev = p.node;
                    p.node = previousNode;
                    previousNode = prev;
                }

                availableNodes.Remove(p.node);
                PlacePlayerObject(p.obj, p.node.worldPosition);
                

            }
        }

        void SetPlayerDirection()
        {
            if (up)
            {
                SetDirection(Direction.UP);
            }
            else if (down)
            {
                SetDirection(Direction.DOWN);
            }
            else if (left)
            {
                SetDirection(Direction.LEFT);
            }
            else if (right)
            {
                SetDirection(Direction.RIGHT);
            }
        }

        void SetDirection(Direction d)
        {
            if (!isOpposite(d))
            {
                curDirection = d;
                timer = MoveRate + 1;
            }


        }
        #endregion

        #region Utilities


        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * .5f;
            obj.transform.position = pos;
        }

        Node GetNode(int x, int y)
        {
            if(x < 0 || x > maxWidth-1 || y < 0 || y > maxHeight-1)
            {
                return null;
            }

            return grid[x, y];
        }

        private void RandomApple()
        {
            int ran = Random.Range(0, availableNodes.Count);

            Node n = availableNodes[ran];
            PlacePlayerObject(apple, n.worldPosition);
            
            appleNode = n;
        }

        bool isOpposite(Direction d)
        {
            switch (d)
            {
                default:
                case Direction.UP:
                    if (curDirection == Direction.DOWN)
                    {
                        return true;
                    }
                    else
                        return false;
                    
                case Direction.DOWN:
                    if (curDirection == Direction.UP)
                    {
                        return true;
                    }
                    else
                        return false;
                   
                case Direction.LEFT:
                    if (curDirection == Direction.RIGHT)
                    {
                        return true;
                    }
                    else
                        return false;
                case Direction.RIGHT:
                    if (curDirection == Direction.LEFT)
                    {
                        return true;
                    }
                    else
                        return false;
            }
        }

        bool isTailNode(Node n)
        {
            for (int i = 0; i < tail.Count; i++)
            {
                if (tail[i].node == n)
                    return true;
                
                    
            }
            return false;
        }

        bool isValidDirection(Node targetNode)
        {
            return targetNode == prevPlayerNode;
                 

        }

        TailNode CreateTailNode(int x, int y)
        {
            TailNode s = new TailNode();
            s.node = GetNode(x, y);
            s.obj = new GameObject("tail");
            s.obj.transform.parent = tailParent.transform;
            s.obj.transform.position = s.node.worldPosition;
            s.obj.transform.localScale = Vector3.one * .95f;
            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
            r.sprite = CreateSprite(playerColor);
            r.sortingOrder = 1;

            return s;
        }

        Sprite CreateSprite(Color targetColor)
        {
            Texture2D txt = new Texture2D(1, 1);
            txt.SetPixel(0, 0, targetColor);
            txt.Apply();
            txt.filterMode = FilterMode.Point;
            Rect rect = new Rect(0, 0, 1, 1);
            return Sprite.Create(txt, rect, Vector2.one *.5f, 1, 0, SpriteMeshType.FullRect);

        }
        #endregion
    }

}
