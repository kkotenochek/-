using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Курсовой_проект
{
    public partial class Form1 : Form
    {
        const int MapSize = 8; //размер поля
        int[,] map = new int[MapSize, MapSize];
        const int CellSize = 65; //размер ячейки

        int CurrentPlayer;//переменная, отвечающая за текущего игрока
        Button PreviousButton;//кнопка, записывающая предыдущую нажатую кнопку
        Button PressedButton;
       
        bool IsContinue = false;//может ли шашка продолжать рубить после того как срубила уже одну шашку
        bool IsMoving; //проверяем, находится ли шашка в процессе ходьбы
        List<Button> ButtonsClose = new List<Button>();
        Button[,] buttons = new Button[MapSize, MapSize];
        int CountEatenSteps = 0;
        static Random rand = new Random();

        Image WhiteCheckers;
        Image BlackCheckers;
        Thread th;

        public Form1()
        {
            InitializeComponent();

            WhiteCheckers = new Bitmap(new Bitmap(@"Ресурсы/whitechecker-removebg1.png"), new Size(CellSize - 10, CellSize - 10));
            BlackCheckers = new Bitmap(new Bitmap(@"Ресурсы/blackchecker-removebg-preview.png"), new Size(CellSize - 10, CellSize - 10));

            Text = "Шашки";
            Init();
        }
        #region Инициализация игры
        public void Init()//функция, отвечающая за инициализацию игры
        {
            CurrentPlayer = 1;
            IsMoving = false;
            PreviousButton = null;

            map = new int[MapSize, MapSize] {
                {0,1,0,1,0,1,0,1 },
                { 1,0,1,0,1,0,1,0 },
                { 0,1,0,1,0,1,0,1 },
                { 0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0 },
                { 2,0,2,0,2,0,2,0 },
                { 0,2,0,2,0,2,0,2 },
                { 2,0,2,0,2,0,2,0 },
            };
            
            
            CreateMap();
        }
        #endregion

        #region Создание игрового поля
        public void CreateMap()//функция для создания игрового поля
        {
            this.Width = (MapSize+3) * CellSize;
            this.Height =(MapSize+3) * CellSize;
            
            for(int i=0; i < MapSize; i++)
            {
                for(int j = 0; j<MapSize; j++)
                {
                    Button button = new Button(); //создание кнопки
                    button.Location = new Point(j * CellSize, i * CellSize);//изменяем положение кнопки относительно параметров поля
                    button.Size = new Size(CellSize, CellSize);//задаем размер кнопки, равный размеру ячейки
                    button.Click += new EventHandler(ButtonProcessing);
                    if (map[i, j] == 1)
                    {
                        button.Image = BlackCheckers;
                    }

                    if (map[i,j]==2)
                    {
                        button.Image = WhiteCheckers;
                    }

                    button.BackColor = GetPreviousButtonColour(button);
                    button.ForeColor = Color.Red;

                    buttons[i, j] = button;

                    this.Controls.Add(button);//добавляем кнопку в коллекцию элементов 
                }
               
            }

        }
        #endregion

        #region Перезапуск игры
        public void ResetGame()
        {
            int winner = GetWinner();
            
            if (winner!=0)//если есть победитель, запускаем по-новой
            {
                if (winner==2) MessageBox.Show("     Победил компьютер. Начните новую игру");
                if (winner==1) MessageBox.Show("     Победили вы. Начните новую игру");
            }
        }
        #endregion

        #region Победитель
        public int GetWinner()
        {
            int winner = 0;
            bool Player1 = false;
            bool Player2 = false;

            for (int i = 0; i < MapSize; i++)//если на поле еще есть шашки 1 или 2 игрока, то продолжаем
            {
                for (int j = 0; j < MapSize; j++)
                {
                    if (map[i, j] == 1)
                        Player1 = true;
                    if (map[i, j] == 2)
                        Player2 = true;
                }
            }

            if (Player1 && Player2) return 0;//победителя нет
            else if (Player1) return 1;//победитель - 1 игрок
            else return 2;//победитель - 2 игрок
        }
        #endregion

        #region Смена игрока
        public void SwitchPlayer()//функция, отвечающая за смену игрока
        {
            CurrentPlayer = CurrentPlayer == 1 ? 2 : 1;//меняем игрока
            ResetGame();
        }
        #endregion

        #region Смена цвета предыдущей кнопки
        public Color GetPreviousButtonColour(Button PreviousButton)//функция смены цвета предыдущей кнопки
        {
            if ((PreviousButton.Location.Y/CellSize % 2) != 0)
            {
                if ((PreviousButton.Location.X / CellSize % 2) == 0)
                {
                    return Color.SkyBlue;
                }
            }

            if ((PreviousButton.Location.Y / CellSize) % 2 == 0)
            {
                if ((PreviousButton.Location.X / CellSize) % 2 != 0)
                {
                    return Color.SkyBlue;
                }
            }

            return Color.White;
        }
        #endregion

        #region Убираем мерцание с формы

        [DllImport("user32.dll")]

        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        private const int WM_SETREDRAW = 11;

        #endregion

        #region Обработка нажатых кнопок
        public void ButtonProcessing(object sender, EventArgs e)//функция обработки нажатых кнопок
        {
            if (PreviousButton != null)
            {
                PreviousButton.BackColor = GetPreviousButtonColour(PreviousButton);
            }

             PressedButton = sender as Button;
             if(map[PressedButton.Location.Y/CellSize, PressedButton.Location.X / CellSize]!=0 && map[PressedButton.Location.Y / CellSize, PressedButton.Location.X / CellSize] == CurrentPlayer)
             {
                CloseSteps();
                PressedButton.BackColor = Color.Pink;
                DeactivateAllButtons();
                PressedButton.Enabled = true;
                CountEatenSteps = 0;
                if(PressedButton.Text == "Дамка")
                {
                    PressButton(PressedButton.Location.Y / CellSize, PressedButton.Location.X / CellSize, false);
                }
                else
                {
                    PressButton(PressedButton.Location.Y / CellSize, PressedButton.Location.X / CellSize);
                }

                if (IsMoving)
                {
                    CloseSteps();
                    PressedButton.BackColor = GetPreviousButtonColour(PressedButton);
                    ShowPossibleSteps();
                    IsMoving = false;
                }
                else
                    IsMoving = true;
            }
            else
            {
                if(IsMoving)
                {
                    IsContinue = false;//сделали ход и теперь смотрим. Если разница в ходе больше 1, значит мы сделали съедобный ход, тогда можем продолжать. Если меньше 1, то это обычный ход
                    if (Math.Abs(PressedButton.Location.X / CellSize - PreviousButton.Location.X / CellSize) > 1)
                    {
                        IsContinue = true;
                        DeleteEaten(PressedButton, PreviousButton);
                    }
                    int temp = map[PressedButton.Location.Y / CellSize, PressedButton.Location.X / CellSize];
                    map[PressedButton.Location.Y / CellSize, PressedButton.Location.X / CellSize] = map[PreviousButton.Location.Y / CellSize, PreviousButton.Location.X / CellSize];
                    map[PreviousButton.Location.Y / CellSize, PreviousButton.Location.X / CellSize] = temp;
                    PressedButton.Image = PreviousButton.Image;
                    PreviousButton.Image = null;
                    PressedButton.Text = PreviousButton.Text;
                    PreviousButton.Text = "";
                    King(PressedButton);//может ли быть преобразована в дамку
                    CountEatenSteps = 0;//обнуляем количество съедобных шагов
                    IsMoving = false;
                    CloseSteps();
                    DeactivateAllButtons();
                    if (PressedButton.Text == "Дамка")//для какого типа шашек активируем шаги
                        PressButton(PressedButton.Location.Y / CellSize, PressedButton.Location.X / CellSize, false);
                    else PressButton(PressedButton.Location.Y / CellSize, PressedButton.Location.X / CellSize);

                    if (CountEatenSteps == 0 || !IsContinue)//нет последующих съедобных ходов, ход завершен
                    {
                        CloseSteps();//закрываем все шаги
                        SwitchPlayer();
                        ShowPossibleSteps();
                        IsContinue = false;
                        if (GetWinner() != 0)//если есть победитель, то ход компьютера нужно остановить
                            return;

                        SendMessage(this.Handle, WM_SETREDRAW, false, 0);
                        if (CurrentPlayer == 2)
                        {
                            int index, position;

                            List<Button> EatenMoves = new List<Button>();
                            List<Button> PossiblePosition = new List<Button>();
                            for(int i = 0; i < MapSize; i++)
                            {
                                for(int j = 0; j<MapSize; j++)
                                {
                                    if(buttons[i,j].Enabled == true)
                                    {
                                        EatenMoves.Add(buttons[i, j]);
                                    }
                                }
                            }
                            if (EatenMoves.Count != 0)
                            {
                                bool CanMove = false; //можно ли двигать выбранную шашку
                                while (!CanMove)
                                {
                                    index = rand.Next(0, EatenMoves.Count);
                                    var element = EatenMoves[index];
                                    element.PerformClick();

                                    for (int i = 0; i < MapSize; i++)
                                    {
                                        for (int j = 0; j < MapSize; j++)
                                        {
                                            if (buttons[i, j].BackColor == Color.Yellow)
                                            {
                                                PossiblePosition.Add(buttons[i, j]);
                                            }
                                        }
                                    }

                                    CanMove = PossiblePosition.Count != 0;
                                    if (CanMove)
                                    {
                                        position = rand.Next(0, PossiblePosition.Count);
                                        var place = PossiblePosition[position];
                                        place.PerformClick();
                                    }
                                    
                                }
                            }
                        }
                        SendMessage(this.Handle, WM_SETREDRAW, true, 0);
                        this.Refresh();
                    }
                    else if (IsContinue)
                    {
                        PressedButton.BackColor = Color.Pink;
                        PressedButton.Enabled = true;
                        IsMoving = true;
                    }
                }
            }
             PreviousButton = PressedButton;
            
        }
        #endregion

        #region Выход за пределы карты
        public bool IsInsideBorders(int ti, int tj)//проверка на выход за пределы карты
        {
            if (ti >= MapSize || tj >= MapSize || ti < 0 || tj < 0) return false;
            else return true;
        }
        #endregion

        #region Выключение кнопок
        public void DeactivateAllButtons()//выключаем все кнопки
        {
            for(int i = 0; i < MapSize; i++)
            {
                for(int j = 0; j < MapSize; j++)
                {
                    buttons[i, j].Enabled = false;
                }
            }
        }
        #endregion

        #region Включение кнопок
        public void ActivateAllButtons()//включаем все кнопки
        {
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    buttons[i, j].Enabled = true;
                }
            }
        }
        #endregion

        #region Закрытие "съедобных" шагов после хода
        public void CloseSteps()//закрываем "съедобные" шаги для шашки после того, как она осуществила ход, и открываем всю карту. Все ячейки снова цвета карты
        {
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    buttons[i, j].BackColor = GetPreviousButtonColour(buttons[i, j]);
                }
            }
        }
        #endregion

        #region Дальнейший путь шашки
        public bool Path(int ti, int tj, bool IsBackwardMove=false)//дальнейший путь
        {
            if (map[ti, tj] == 0 && !IsContinue&&!IsBackwardMove)
            {
                buttons[ti, tj].BackColor = Color.Yellow;
                buttons[ti, tj].Enabled = true;
                ButtonsClose.Add(buttons[ti, tj]);//массив простых ходов
            }
            else
            {

                if (map[ti, tj] != CurrentPlayer)
                {
                    if (PressedButton.Text == "Дамка")
                        ShowEat(ti, tj, false);
                    else ShowEat(ti, tj);
                }

                return false;
            }
            return true;
        }
        #endregion

        #region Закрытие несъедобных ходов
        public void CloseSimpleSteps(List<Button> ButtonsClose)//закрываем несъедобные ходы
        {
            if (ButtonsClose.Count > 0)
            {
                for (int i = 0; i < ButtonsClose.Count; i++)
                {
                    ButtonsClose[i].BackColor = GetPreviousButtonColour(ButtonsClose[i]);
                    ButtonsClose[i].Enabled = false;
                }
            }
        }
        #endregion

        #region Действия при нажатии на кнопку
        public void PressButton (int iCurrChecker, int jCurrChecker, bool isOnestep = true)//при нажатии на кнопку
        {
            ButtonsClose.Clear();
            ShowDiagonal(iCurrChecker, jCurrChecker, isOnestep);//после вызова в процессе игры будет добавлять какие-то простые ходы в список ButtonClose
            if (CountEatenSteps > 0)
                CloseSimpleSteps(ButtonsClose);//закрываем все простые ходы, оставляем только съедобные
        }
        #endregion

        #region Построение диагонали для хода
        public void ShowDiagonal(int IcurrChecker, int JcurrChecker, bool isOneStep = false)//строит диагональ, куда можно сходить
        {
            int j = JcurrChecker + 1;
            for (int i = IcurrChecker - 1; i >= 0; i--)
            {
                bool IsBackwardMove = CurrentPlayer == 1 && isOneStep && !IsContinue;
                if (IsInsideBorders(i, j))
                {
                    if (!Path(i, j, IsBackwardMove))
                        break;
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrChecker - 1;
            for (int i = IcurrChecker - 1; i >= 0; i--)
            {
                bool IsBackwardMove = CurrentPlayer == 1 && isOneStep && !IsContinue;
                if (IsInsideBorders(i, j))
                {
                    if (!Path(i, j, IsBackwardMove))
                        break;
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrChecker - 1;
            for (int i = IcurrChecker + 1; i < 8; i++)
            {
                bool IsBackwardMove = CurrentPlayer == 2 && isOneStep && !IsContinue;
                if (IsInsideBorders(i, j))
                {
                    if (!Path(i, j, IsBackwardMove))
                        break;
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrChecker + 1;
            for (int i = IcurrChecker + 1; i < 8; i++)
            {
                bool IsBackwardMove = CurrentPlayer == 2 && isOneStep && !IsContinue;
                if (IsInsideBorders(i, j))
                {
                    if (!Path(i, j, IsBackwardMove))
                        break;
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }
        }
        #endregion

        #region Следующий съедобный ход
        public void ShowEat(int i, int j, bool isOneStep = true)//строит следующий съедобный ход
        {
            int directionX = i - PressedButton.Location.Y / CellSize;//находим направление, относительно которого сходили сейчас шашкой
            int directionY = j - PressedButton.Location.X / CellSize;//находим направление, относительно которого сходили сейчас шашкой
            directionX = directionX < 0 ? -1 : 1; //так как направление может быть не только 1 и -1, а больше или меньше, то мы с помощью тернарного оператора присваиваем значение либо -1, либо 1
            directionY = directionY < 0 ? -1 : 1;
            int il = i;
            int jl = j;
            bool isEmpty = true;//возможно ли простроить какой-то съедобный ход
            while (IsInsideBorders(il, jl))
            {
                if (map[il, jl] != 0 && map[il, jl] != CurrentPlayer)//если в этих индексах карта ненулевая и это не текущий игрок, то значит там что-то есть и клетка не пустая
                {
                    isEmpty = false;
                    break;
                }
                il += directionX;//в зависимости от этих переменных двигаемся по карте
                jl += directionY;//в зависимости от этих переменных двигаемся по карте 

                if (isOneStep)
                    break;
            }
            if (isEmpty)//никаких съедобных ходов не нашло, значит заканчиваем выполнение функции
                return;
            List<Button> ButtonsClose = new List<Button>();//создаем список кнопок, чтобы при необходимости какие-то кнопки можно было выключить
            int CountEatenSteps = 0;//подсчет съедобных ходов
            bool CloseSimple = false;//нужно ли закрыть несъедобные ходы
            int ik = il + directionX;
            int jk = jl + directionY;
            while (IsInsideBorders(ik, jk))
            {
                if (map[ik, jk] == 0)//ячейка нулевая, то есть можем туда сходить
                {
                    if (IsButtonHasEatenStep(ik, jk, isOneStep, new int[2] { directionX, directionY }))//проверяем, есть ли еще нулевые клетки за этой ячейкой
                    {
                        CloseSimple = true;//если возвращает true, то мы закрываем все простые(несъедобные)шаги
                    }
                    else
                    {
                        ButtonsClose.Add(buttons[ik, jk]);//если вернуло false, то записываем эту кнопку, так как подразумевается, что она имеет несъедобный ход
                    }
                    buttons[ik, jk].BackColor = Color.Yellow;
                    buttons[ik, jk].Enabled = true;
                    CountEatenSteps++;
                }
                else break;//если ячейка ненулевая, то завершаем цикл
                if (isOneStep)//так как смотрим только на одну клетку, то выходим из цикла
                    break;
                jk += directionY;//увеличиваем на направление, чтобы просмотреть все направления до конца карты и проверить все ячейки
                ik += directionX;//увеличиваем на направление, чтобы просмотреть все направления до конца карты и проверить все ячейки
            }
            if (CloseSimple && ButtonsClose.Count > 0)//если есть съедобные ходы, то заркываем простые ходы
            {
                CloseSimpleSteps(ButtonsClose);
            }

        }
        #endregion

        #region Есть ли у шашки съедобный ход
        public bool IsButtonHasEatenStep(int ICurrChecker, int JCurrChecker, bool IsOneStep, int []direction) //функция, проверяющая, есть ли у шашки какой-то съедобный ход, то есть может ли она что-то срубить
        {
            bool IsBackwardMove=false;
            bool EatenStep = false;
            int j = JCurrChecker + 1;
            for(int i = ICurrChecker - 1; i >= 0; i--)//i-- идем по полю вверх
            {
                IsBackwardMove = CurrentPlayer == 1 && IsOneStep && !IsContinue;//если первый игрок сделал один ход и он не может больше рубить, то есть нет доступных ходов, то прерываем 
                if (direction[0] == 1 && direction[1] == -1 && !IsOneStep) break; // шашка сходила вниз и влево
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != CurrentPlayer)
                    {
                        EatenStep = true;
                        IsBackwardMove = true;
                        if (!IsInsideBorders(i - 1, j + 1))//вверх вправо (если находится за пределами карты, то осуществить ход нельзя)
                        {
                            EatenStep = false;
                            IsBackwardMove = false;
                        }
                        else if (map[i - 1, j + 1] != 0) //если в пределах карты, смотрим, чтобы карта с этими параметрами была ненулевой (если ненулевая, значит за этой шашкой стоит другая)
                        {
                            EatenStep = false;
                            IsBackwardMove = false;
                        }
                        else return EatenStep&&IsBackwardMove;
                    }
                    if (j < 7)
                        j++;
                    else break;

                    if (IsOneStep)
                        break;
                }
                
            }

            j = JCurrChecker - 1;
            for (int i = ICurrChecker - 1; i >= 0; i--)//i-- идем по полю вверх
            {
                IsBackwardMove = CurrentPlayer == 1 && IsOneStep && !IsContinue;//если первый игрок сделал один ход и он не может больше рубить, то есть нет доступных ходов, то прерываем 
                if (direction[0] == 1 && direction[1] == 1 && !IsOneStep) break; // шашка сходила вниз и вправо
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != CurrentPlayer)
                    {
                        EatenStep = true;
                        IsBackwardMove = true;
                        if (!IsInsideBorders(i - 1, j - 1))//вверх влево (если находится за пределами карты, то осуществить ход нельзя)
                        {
                            EatenStep = false;
                            IsBackwardMove = false;
                        }
                        else if (map[i - 1, j - 1] != 0) //если в пределах карты, смотрим, чтобы карта с этими параметрами была ненулевой (если ненулевая, значит за этой шашкой стоит другая)
                        {
                            EatenStep = false;
                            IsBackwardMove = false;
                        }
                        else return EatenStep&&IsBackwardMove;
                    }
                    if (j > 0)
                        j--;
                    else break;

                    if (IsOneStep)
                        break;
                }

            }
            #region Для компьютера
            //для компьютера
            j = JCurrChecker - 1;
            for (int i = ICurrChecker + 1; i <8; i++)//идем по полю вниз
            {
                IsBackwardMove = CurrentPlayer == 2 && IsOneStep && !IsContinue;//если первый игрок сделал один ход и он не может больше рубить, то есть нет доступных ходов, то прерываем 
                if (direction[0] == -1 && direction[1] == 1 && !IsOneStep) break; // шашка сходила вверх и вправо
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != CurrentPlayer)
                    {
                        EatenStep = true;
                        IsBackwardMove = true;
                        if (!IsInsideBorders(i + 1, j - 1))//вниз влево (если находится за пределами карты, то осуществить ход нельзя)
                        {
                            EatenStep = false;
                            IsBackwardMove = false;
                        }
                        else if (map[i + 1, j - 1] != 0) //если в пределах карты, смотрим, чтобы карта с этими параметрами была ненулевой (если ненулевая, значит за этой шашкой стоит другая)
                        {
                            EatenStep = false;
                            IsBackwardMove = false;
                        }
                        else return EatenStep&&IsBackwardMove;
                    }
                    if (j > 0)
                        j--;
                    else break;

                    if (IsOneStep)
                        break;
                }

            }

            j = JCurrChecker + 1;
            for (int i = ICurrChecker - 1; i<8; i++)//идем по полю вниз
            {
                IsBackwardMove = CurrentPlayer == 2 && IsOneStep && !IsContinue;//если первый игрок сделал один ход и он не может больше рубить, то есть нет доступных ходов, то прерываем 
                if (direction[0] == -1 && direction[1] == -1 && !IsOneStep) break; // шашка сходила вверх и влево
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != CurrentPlayer)
                    {
                        EatenStep = true;
                        IsBackwardMove = true;
                        if (!IsInsideBorders(i + 1, j + 1))//вниз вправо (если находится за пределами карты, то осуществить ход нельзя)
                        {
                            EatenStep = false;
                            IsBackwardMove = false;
                        }
                        else if (map[i + 1, j + 1] != 0) //если в пределах карты, смотрим, чтобы карта с этими параметрами была ненулевой (если ненулевая, значит за этой шашкой стоит другая)
                        {
                            EatenStep = false;
                            IsBackwardMove = false;
                        }
                        else return EatenStep&&IsBackwardMove;
                    }
                    if (j < 7)
                        j++;
                    else break;

                    if (IsOneStep)
                        break;
                }

            }
            return EatenStep;
        }
        #endregion
        #endregion

        #region Удаление "съеденных" шашек
        public void DeleteEaten(Button StartButton, Button EndButton)
        {
            int distance = Math.Abs(EndButton.Location.Y / CellSize - StartButton.Location.Y / CellSize);//расстояние между двумя кнопками
            int StartIndexX = EndButton.Location.Y / CellSize - StartButton.Location.Y / CellSize;
            int StartIndexY = EndButton.Location.X / CellSize - StartButton.Location.X / CellSize;
            StartIndexX = StartIndexX < 0 ? -1 : 1;//определяем направление
            StartIndexY = StartIndexY < 0 ? -1 : 1;
            int CurrDistance = 0;
            int i = StartButton.Location.Y / CellSize + StartIndexX;
            int j = StartButton.Location.X / CellSize + StartIndexY;
            while (CurrDistance < distance - 1)
            {
                map[i, j] = 0;
                buttons[i, j].Image = null;
                buttons[i, j].Text = "";
                i += StartIndexX;
                j += StartIndexY;
                CurrDistance++;
            }

        }
        #endregion

        #region Является ли шашка дамкой
        public void King (Button button)
        {
            if (map[button.Location.Y / CellSize, button.Location.X / CellSize] == 1 && button.Location.Y / CellSize == MapSize - 1)//если шашка относится к 1 игроку и находится в краю карты (снизу)
            {
                button.Text = "Дамка";

            }
            if (map[button.Location.Y / CellSize, button.Location.X / CellSize] == 2 && button.Location.Y / CellSize == 0)//если шашка относится к 2 игроку и находится в краю карты (сверху)
            {
                button.Text = "Дамка";
            }
        }
        #endregion

        #region Подсвечивание возможных ходов после нажатия на шашку
        public void ShowPossibleSteps()//выбираем шашку, после нажатия на нее подсвечивается возможный ход
        {
            bool isOneStep = true;//дамка или нет
            bool isEatenStep = false;//есть ли съедобный ход
            DeactivateAllButtons();
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    if (map[i, j] == CurrentPlayer)
                    {
                        if (buttons[i, j].Text == "Дамка")
                            isOneStep = false;
                        else isOneStep = true;
                        if (IsButtonHasEatenStep(i, j, isOneStep, new int[2] { 0, 0 }))
                        {
                            isEatenStep = true;//есть ход
                            buttons[i, j].Enabled = true;//включаем кнопку
                        }
                    }
                }
            }
            if (!isEatenStep)//если нет съедобных ходов
                ActivateAllButtons();//активируем все кнопки и пользователь может выбирать то, что ему хочется
        }
        #endregion

        #region Кнопка начала новой игры после окончания игрового процесса
        private void ButtonRestart_Click(object sender, EventArgs e)//кнопка начала новой игры
        {
            this.Close();
            th = new Thread(Open);
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

        }

        public void Open(object obj)
        {
            Application.Run(new Form1());
        }
        #endregion

        #region Кнопка выхода в главное меню
        private void button1_Click(object sender, EventArgs e)//кнопка выхода в главное меню
        {
            this.Close();
            th = new Thread(Open1);
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }
        public void Open1(object obj)
        {
            Application.Run(new Form2());
        }
        #endregion

       
    }
}
