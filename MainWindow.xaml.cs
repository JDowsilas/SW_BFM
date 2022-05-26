using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Media;
using System.IO;

namespace SW_BFM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region variables
        DispatcherTimer gameTimer = new DispatcherTimer(); //game main timer
        DispatcherTimer specialTimer = new DispatcherTimer(); // timer for special attack
        DispatcherTimer bossTimer = new DispatcherTimer(); // boss shot timer
        DispatcherTimer bosscritTimer = new DispatcherTimer(); // boss critshot timer
        SoundPlayer sp;
        List<Rectangle> itemRemover = new List<Rectangle>(); //list for removing items from canvas
        Random random = new Random();

        int playerSpeed = 25; //speed of the player
        int enemySpeed = 10; //speed of the enemies
        int enemyCounter = 100; //counter for enemies
        int limit = 50; //limit of spawning enemies
        public static int score = 0;
        int damage = 0;

        bool bossFight;
        int bossHP = 150;
        bool bossStop;
        bool bossWin;
        int moveBoss = 0;
        Rectangle bossRect;
        Rectangle bossBar;



        int enemySpriteCounter = 0;
        int enemyHP = 1;
        Random rand = new Random();

        bool moveLeft, moveRight, moveUp, moveDown; //player controls   
        bool gameON = true; //game on switch

        Rect playerHitBox;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;

            specialTimer.Interval = TimeSpan.FromMilliseconds(150);
            specialTimer.Tick += SpecialCritLoop;

            bossTimer.Interval = TimeSpan.FromMilliseconds(80);
            bossTimer.Tick += BossLoop;

            bosscritTimer.Interval = TimeSpan.FromMilliseconds(4000);
            bosscritTimer.Tick += BossCritLoop;

            GameCanvas.Focus();
            sp = new SoundPlayer(SW_BFM.Properties.Resources.theme);
            sp.PlayLooping();

            //background settings
            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"images\background.png"));
            bg.TileMode = TileMode.Tile;
            bg.Viewport = new Rect(0, 0, 0.15, 0.15);
            bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            GameCanvas.Background = bg;

            //player
            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"images\Sokol.gif"));
            player.Fill = playerImage;

            player.Visibility = Visibility.Hidden;
            gameOverLabel.Visibility = Visibility.Hidden;
            gui.Visibility = Visibility.Hidden;
            scoreText.Visibility = Visibility.Hidden;
            hpLabel.Visibility = Visibility.Hidden;
            hpBar.Visibility = Visibility.Hidden;
            specialLabel.Visibility = Visibility.Hidden;
            specialBar.Visibility = Visibility.Hidden;
            bosslabel.Visibility = Visibility.Hidden;

        }
        private void GameLoop(object sender, EventArgs e) //main game loop
        {
            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height); //hitbox of the player
            enemyCounter -= 1;
            scoreText.Content = "SCORE: " + score; //change score every tick
            if (enemyCounter < 0 && bossFight == false)
            {
                MakeEnemies(); //spawn enemies
                enemyCounter = limit;
            }

            #region player movement

            if (moveLeft == true && Canvas.GetLeft(player) > 0)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            }
            if (moveRight == true && Canvas.GetLeft(player) + 110 < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            }
            if (moveUp == true && Canvas.GetTop(player) > Application.Current.MainWindow.Height / 2 + 100)
            {
                Canvas.SetTop(player, Canvas.GetTop(player) - playerSpeed);
            }
            if (moveDown == true && Canvas.GetTop(player) + 120 < Application.Current.MainWindow.Height)
            {
                Canvas.SetTop(player, Canvas.GetTop(player) + playerSpeed);
            }

            #endregion

            foreach (var x in GameCanvas.Children.OfType<Rectangle>())
            {
                if (x is Rectangle && (string)x.Tag == "bullet") //bullet movement
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) - 30);
                    Rect bulletHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (Canvas.GetTop(x) < 70)
                    {
                        itemRemover.Add(x); //delete bullets when they reach the end of screen
                    }

                    foreach (var y in GameCanvas.Children.OfType<Rectangle>())
                    {
                        if (y is Rectangle && (string)y.Tag == "enemy") //shooting enemy
                        {
                            Rect enemyHit = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);

                            if (bulletHitBox.IntersectsWith(enemyHit))
                            {
                                enemyHP -= 1;
                                if (enemyHP <= 0)
                                {
                                    itemRemover.Add(y);
                                    score++;
                                }
                                itemRemover.Add(x);
                            }
                        }
                        if (y is Rectangle && (string)y.Tag == "boss")
                        {
                            Rect bossHit = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);

                            if (bulletHitBox.IntersectsWith(bossHit))
                            {
                                bossHP -= 1;
                                bossBar.Width = bossBar.Width - 1;
                                itemRemover.Add(x);
                                if (bossHP <= 0)
                                {
                                    itemRemover.Add(x);
                                    itemRemover.Add(y);
                                    bossWin = true;

                                }
                            }

                        }
                    }
                }
                if (x is Rectangle && (string)x.Tag == "bossbullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 10);

                    Rect bossBulletHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (Canvas.GetTop(x) > 700)
                    {
                        itemRemover.Add(x);
                    }

                    if (bossBulletHitBox.IntersectsWith(playerHitBox))
                    {
                        itemRemover.Add(x);
                        damage += 5;
                        if (hpBar.Width >= 5)
                        {
                            hpBar.Width -= 5;
                        }
                        else { hpBar.Width -= hpBar.Width; }

                    }

                }
                if (x is Rectangle && (string)x.Tag == "bosscrit")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 2);

                    Rect bossBulletHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (Canvas.GetTop(x) > 700)
                    {
                        itemRemover.Add(x);
                    }

                    if (bossBulletHitBox.IntersectsWith(playerHitBox))
                    {
                        itemRemover.Add(x);
                        damage = 200;
                        hpBar.Width = 0;
                    }

                }
                if (x is Rectangle && (string)x.Tag == "enemy") //enemy movement
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + enemySpeed);

                    if (Canvas.GetTop(x) > 850)
                    {
                        itemRemover.Add(x);
                    }

                    Rect enemyHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    if (playerHitBox.IntersectsWith(enemyHitBox)) //enemy touches player (future damage)
                    {
                        itemRemover.Add(x);
                        damage += 10;
                        hpBar.Width -= 10;
                    }
                }
                if (x is Rectangle && (string)x.Tag == "specialbullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) - 10);
                    Rect specialHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (Canvas.GetTop(x) < 10)
                    {
                        itemRemover.Add(x);
                    }

                    foreach (var y in GameCanvas.Children.OfType<Rectangle>())
                    {
                        if (y is Rectangle && (string)y.Tag == "enemy")
                        {
                            Rect enemyspecialHit = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);

                            if (specialHitBox.IntersectsWith(enemyspecialHit))
                            {
                                itemRemover.Add(y);
                                score++;
                            }
                        }

                    }
                }
            }
            #region enemy speed
            if (score > 5)
            {
                limit = 20;
                enemySpeed = 11;
            }
            if (score > 10)
            {
                limit = 25;
                enemySpeed = 12;
            }
            if (score > 20)
            {
                limit = 25;
                enemySpeed = 13;
            }
            if (score > 25)
            {
                limit = 25;
                enemySpeed = 14;
            }
            if (score > 30)
            {
                limit = 20;
                enemySpeed = 15;
            }
            if (score > 40)
            {
                limit = 20;
                enemySpeed = 16;
            }
            if (score > 50)
            {
                limit = 15;
                enemySpeed = 17;
            }
            if (score > 60)
            {
                limit = 10;
                enemySpeed = 20;
            }
            #endregion

            if (damage > 199)
            {
                gameOverLabel.Visibility = Visibility.Visible;
                gameTimer.Stop();
                GameOver();
            }
            if (bossWin == true)
            {
                gameTimer.Stop();
                bossTimer.Stop();
                bosscritTimer.Stop();
                specialTimer.Stop();
                GameOver();
            }
            foreach (Rectangle i in itemRemover)
            {
                GameCanvas.Children.Remove(i); //removing item from canvas
            }

        }

        private void BossLoop(object sender, EventArgs e)
        {
            if (score > 50)
            {
                bossFight = true;
                BossFight();

                if (moveBoss == 0)
                {
                    Canvas.SetLeft(bossRect, Canvas.GetLeft(bossRect) + rand.Next(1, 3));
                    moveBoss = 1;
                }
                if (moveBoss == 1)
                {
                    Canvas.SetLeft(bossRect, Canvas.GetLeft(bossRect) - rand.Next(1, 3));
                    moveBoss = 0;
                }


                Rectangle bossBullet = new Rectangle
                {
                    Tag = "bossbullet",
                    Height = 15,
                    Width = 4,
                    Fill = Brushes.Red
                };

                Canvas.SetLeft(bossBullet, Canvas.GetLeft(bossRect) + rand.Next(-100, 600));
                Canvas.SetTop(bossBullet, Canvas.GetTop(bossRect) + bossRect.Height / 2);
                GameCanvas.Children.Add(bossBullet);
            }
        }
        private void BossCritLoop(object sender, EventArgs e)
        {
            if (score > 50)
            {
                ImageBrush critImage = new ImageBrush();
                var image = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"images\bossCrit.png"));
                critImage.ImageSource = image;

                Rectangle bossCrit = new Rectangle
                {
                    Tag = "bosscrit",
                    Height = 80,
                    Width = 80,
                    Fill = critImage

                };
                Canvas.SetLeft(bossCrit, Canvas.GetLeft(bossRect) + rand.Next(0, 600));
                Canvas.SetTop(bossCrit, Canvas.GetTop(bossRect) + bossRect.Height / 2);
                GameCanvas.Children.Add(bossCrit);
            }
        }
        private void SpecialCritLoop(object sender, EventArgs e)
        {
            if (specialBar.Width < 200)
            {
                specialBar.Width++;
            }
        }
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Start();
            specialTimer.Start();
            bossTimer.Start();
            bosscritTimer.Start();

            player.Visibility = Visibility.Visible;
            gui.Visibility = Visibility.Visible;
            scoreText.Visibility = Visibility.Visible;
            hpLabel.Visibility = Visibility.Visible;
            hpBar.Visibility = Visibility.Visible;
            specialLabel.Visibility = Visibility.Visible;
            specialBar.Visibility = Visibility.Visible;

            startButton.Visibility = Visibility.Hidden;
            introText.Visibility = Visibility.Hidden;

        }

        private void GameOver()
        {
            sp.Stop();
            SW_BFM.playerSummary ps = new SW_BFM.playerSummary();
            if (bossWin)
            {
                ps.gameOverLabel.Content = "YOU WON";
                sp = new SoundPlayer(SW_BFM.Properties.Resources.win);
                sp.Play();
            }
            else
            {
                sp = new SoundPlayer(SW_BFM.Properties.Resources.game_over);
                sp.Play();
            }
            ps.Show();
            ps.finalScoreLabel.Content = "YOUR SCORE: " + score;

        }

        #region key config
        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Left)
            {
                moveLeft = true;
            }
            if (e.Key == Key.Right)
            {
                moveRight = true;
            }
            if (e.Key == Key.Up)
            {
                moveUp = true;
            }
            if (e.Key == Key.Down)
            {
                moveDown = true;
            }

        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = false;
            }
            if (e.Key == Key.Right)
            {
                moveRight = false;
            }
            if (e.Key == Key.Up)
            {
                moveUp = false;
            }
            if (e.Key == Key.Down)
            {
                moveDown = false;
            }
            if (e.Key == Key.Z)
            {
                if (gameON == true)
                {

                    Rectangle newBullet = new Rectangle
                    {
                        Tag = "bullet",
                        Height = 20,
                        Width = 5,
                        Fill = Brushes.White,
                        Stroke = Brushes.Red
                    };

                    Canvas.SetLeft(newBullet, Canvas.GetLeft(player) + player.Width / 2);
                    Canvas.SetTop(newBullet, Canvas.GetTop(player) - newBullet.Height);
                    GameCanvas.Children.Add(newBullet);
                }

            }
            if (e.Key == Key.X)
            {
                if (specialBar.Width == 200)
                {
                    Rectangle newSpecial = new Rectangle
                    {
                        Tag = "specialbullet",
                        Height = 15,
                        Width = 740,
                        Fill = Brushes.Red,
                        Stroke = Brushes.White

                    };
                    Canvas.SetLeft(newSpecial, 0);
                    Canvas.SetTop(newSpecial, Canvas.GetTop(player) - newSpecial.Height);
                    GameCanvas.Children.Add(newSpecial);
                    specialBar.Width = 1;
                }
            }
        }
        #endregion

        private void MakeEnemies() //make and randomize enemies
        {
            ImageBrush enemySprite = new ImageBrush();
            enemySpriteCounter = rand.Next(1, 6);

            switch (enemySpriteCounter)
            {
                case 1:
                    enemySprite.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"images\Imperium.gif"));
                    enemyHP = 2;
                    break;
                case 2:
                    enemySprite.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"images\MiniBossKappa.gif"));
                    enemyHP = 4;
                    break;
                case 3:
                    enemySprite.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"images\Harambe1.png"));
                    enemyHP = 1;
                    break;
                case 4:
                    enemySprite.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"images\Shrek.png"));
                    enemyHP = 5;
                    break;
                case 5:
                    enemySprite.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"images\Wazowski.gif"));
                    enemyHP = 3;
                    break;
            }

            Rectangle newEnemy = new Rectangle
            {
                Tag = "enemy",
                Height = 80,
                Width = 80,
                Fill = enemySprite,
            };
            Canvas.SetZIndex(newEnemy, 1); //enemies are under GUI
            Canvas.SetTop(newEnemy, -100);
            Canvas.SetLeft(newEnemy, random.Next(30, 630));
            GameCanvas.Children.Add(newEnemy);

        }

        private void BossFight()
        {
            if (bossFight == true && bossStop == false)
            {
                sp = new SoundPlayer(SW_BFM.Properties.Resources.boss);
                sp.PlayLooping();
                bosslabel.Visibility = Visibility.Visible;
                ImageBrush bossImage = new ImageBrush();
                bossImage.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"images\AMOGUS2.png"));

                bossRect = new Rectangle
                {
                    Tag = "boss",
                    Height = 207,
                    Width = 656,
                    Fill = bossImage
                };

                bossBar = new Rectangle
                {
                    Tag = "bossbar",
                    Height = 10,
                    Width = 150,
                    Fill = Brushes.Red,
                };

                Canvas.SetTop(bossBar, 50);
                Canvas.SetLeft(bossBar, 200);
                Canvas.SetZIndex(bossBar, 3);

                Canvas.SetTop(bossRect, 80);
                Canvas.SetLeft(bossRect, 40);

                GameCanvas.Children.Add(bossRect);
                GameCanvas.Children.Add(bossBar);
                bossStop = true;
            }
        }

    }
}
