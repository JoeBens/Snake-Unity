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


        public Transform cameraHolder;

        GameObject mapObject;
        SpriteRenderer sp;

        Node[,] grid;

        List<Node> availableNodes = new List<Node>();


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
            appleColor = Color.red;
            CreateMap();
            PlacePlayer();
            PlaceCamera();
            curDirection = Direction.RIGHT;
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
            player.transform.position = playerNode.worldPosition;
            
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
            SpriteRenderer appleSp = player.AddComponent<SpriteRenderer>();
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
                if(targetNode == appleNode)
                {
                    

                    RandomApple();
                }


                player.transform.position = targetNode.worldPosition;
                playerNode = targetNode;
            }
        }

        void SetPlayerDirection()
        {
            if (up)
            {
                curDirection = Direction.UP;
            }
            else if (down)
            {
                curDirection = Direction.DOWN;
            }
            else if (left)
            {
                curDirection = Direction.LEFT;
            }
            else if (right)
            {
                curDirection = Direction.RIGHT;
            }
        }

        #endregion

        #region Utilities
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

            apple.transform.position = n.worldPosition;
            appleNode = n;
        }

        Sprite CreateSprite(Color targetColor)
        {
            Texture2D txt = new Texture2D(1, 1);
            txt.SetPixel(0, 0, targetColor);
            txt.Apply();
            txt.filterMode = FilterMode.Point;
            Rect rect = new Rect(0, 0, 1, 1);
            return Sprite.Create(txt, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);

        }
        #endregion
    }

}
