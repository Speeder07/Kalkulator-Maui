using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Kalkulator
{
    public class HistoryContainer
    {
        public string Equasion { get; set; }
        public string Result { get; set; }
        public CalculationChain.CalculationBlock.EqualsCommand Data;

    }
    public class CalculationChain
    {
        Action<string, string> display;
        Stack<CalculationCommand> commands;
        Stack<CalculationBlock> blocks;
        public CalculationChain(Action<string, string> display)
        {
            this.display = display;
            this.commands = new Stack<CalculationCommand>();
            this.blocks = new Stack<CalculationBlock>();
        }

        public event Action<HistoryContainer> OnEquals;

        void EqualsSave(CalculationBlock.EqualsCommand calculationCommand)
        {
            StringBuilder newText = new StringBuilder();
            IEnumerable<CalculationBlock> calculationBlocks = calculationCommand.savesBlocks.Reverse();
            foreach (CalculationBlock block in calculationBlocks)
            {
                newText.Append(block.DisplayValue);
            }

            this.OnEquals?.Invoke(new HistoryContainer
            {
                Equasion = newText.ToString(),
                Result = calculationCommand.result.ToString(),
                Data = calculationCommand
            });
        }

        public void LoadHistory(HistoryContainer history)
        {
            this.blocks.Clear();
            this.blocks.Push(new CalculationBlock.NumericBlock(this, history.Result));
            this.commands = new Stack<CalculationCommand>(history.Data.savedCommands.Reverse());
            this.Display();
        }

        public void Display()
        {
            StringBuilder newText = new StringBuilder();
            IEnumerable<CalculationBlock> calculationBlocks = this.blocks.Reverse();
            foreach (CalculationBlock block in calculationBlocks)
            {
                newText.Append(block.DisplayValue);
            }
            this.display(newText.ToString(), "");
        }


        public void InputOperation(CalculationBlock.OperationBlock.CalcOperation operation)
        {
            CalculationCommand newOperationCommand = new CalculationBlock.OperationBlock.Command(this, operation);
            if( newOperationCommand.Execute()) commands.Push(newOperationCommand);
            Display();
        }
        public void InputNumber(byte number)
        {
            CalculationCommand newCommand = new CalculationBlock.NumericBlock.Command(this, number);
            if (newCommand.Execute()) commands.Push(newCommand);
            Display();
        }
        public void InputEquals()
        {
            CalculationBlock.EqualsCommand calculationCommand = new CalculationBlock.EqualsCommand(this);
            if (calculationCommand.Execute())
            {
                commands.Push(calculationCommand);
                EqualsSave(calculationCommand);
            }
            Display();
        }
        public void TurnBack(){
            if (commands.Count!=0) commands.Pop().UnExecute();
            Display();
        }


        internal interface CalculationCommand
        {
            public bool Execute();
            public void UnExecute();
        }


        public abstract class CalculationBlock
        {
            CalculationChain chain;
            public CalculationBlock(CalculationChain parent)
            {
                this.chain = parent;
            }

            public abstract string DisplayValue { get; }


            public class EqualsCommand : CalculationCommand
            {
                internal CalculationChain chain;
                internal Stack<CalculationBlock> savesBlocks;
                internal Stack<CalculationCommand> savedCommands;
                public double result;
                internal EqualsCommand(CalculationChain chain) {
                    this.chain = chain;
                    this.savesBlocks = new Stack<CalculationBlock>();
                }

                ImmutableDictionary<OperationBlock.CalcOperation, byte> priotityDict = new Dictionary<OperationBlock.CalcOperation, byte>
                {{OperationBlock.CalcOperation.ADD,0},{OperationBlock.CalcOperation.SUB,0},
                {OperationBlock.CalcOperation.MLT,1},{OperationBlock.CalcOperation.DIV,1}}
            .ToImmutableDictionary();

                int Calculate()
                {
                    List<CalculationBlock> copiedBlocks = new List<CalculationBlock>(this.chain.blocks.Reverse().ToList());

                    while (copiedBlocks.Count > 2)
                    {
                        CalcCollapse(copiedBlocks, FindNextOperation(copiedBlocks).index);
                    }
                    return Int32.Parse(copiedBlocks[0].DisplayValue);
                }

                (int priority, int index) FindNextOperation(List<CalculationBlock> calculationBlocks)
                {
                    (int priority, int index) currrent = (-1, -1);
                    for (int i = 0; i < calculationBlocks.Count; i++)
                    {
                        if (calculationBlocks[i] is not OperationBlock) continue;

                        OperationBlock block = calculationBlocks[i] as OperationBlock;
                        if (currrent.priority < priotityDict[block.calcOperation])
                        {
                            currrent = (priotityDict[block.calcOperation], i);
                        }

                    }
                    return currrent;
                }

                void CalcCollapse(List<CalculationBlock> calculationBlocks, int index)
                {
                    int value1 = Int32.Parse(((NumericBlock)calculationBlocks[index - 1]).DisplayValue);
                    int value2 = Int32.Parse(((NumericBlock)calculationBlocks[index + 1]).DisplayValue);

                    double result = 0;
                    switch (((OperationBlock)calculationBlocks[index]).calcOperation)
                    {
                        case OperationBlock.CalcOperation.ADD:
                            result = value1 + value2;
                            break;
                        case OperationBlock.CalcOperation.SUB:
                            result = value1 - value2;
                            break;
                        case OperationBlock.CalcOperation.MLT:
                            result = value1 * value2;
                            break;
                        case OperationBlock.CalcOperation.DIV:
                            result = value1 / value2;
                            break;
                    }

                    calculationBlocks[index] = new NumericBlock(chain, result.ToString());
                    calculationBlocks.RemoveAt(index + 1);
                    calculationBlocks.RemoveAt(index - 1);
                }

                void ResetCalculator()
                {
                    this.savesBlocks = new Stack<CalculationBlock>(chain.blocks);
                    this.savedCommands = new Stack<CalculationCommand>(chain.commands);
                    this.savedCommands.Push(this);
                    this.result = Calculate();
                    chain.blocks.Clear();
                    chain.blocks.Push(new NumericBlock(chain, this.result.ToString()));
                }
                public bool Execute()
                {
                    if (chain.blocks.Count == 0) return false;
                    else if (chain.blocks.Peek() is OperationBlock)
                    {
                        chain.blocks.Push(new NumericBlock(chain, "0"));
                        ResetCalculator();
                        return true;
                    }
                    else if (chain.blocks.Peek() is NumericBlock)
                    {
                        ResetCalculator();
                        return true;
                    }
                    return false;
                }
                public void UnExecute()
                {
                    chain.blocks.Clear();
                    chain.blocks = new Stack<CalculationBlock>(this.savesBlocks);
                }
            }
            

            public class NumericBlock : CalculationBlock
            {
                private StringBuilder value;

                public override string DisplayValue { get => this.value.ToString(); }

                public NumericBlock(CalculationChain parent) : base(parent)
                {
                    this.value = new StringBuilder();
                }

                public NumericBlock(CalculationChain parent, string baseValue) : base(parent)
                {
                    this.value = new StringBuilder();
                    this.value.Append(baseValue);
                }

                public class Command : CalculationCommand
                {
                    CalculationChain chain;
                    byte number;
                    bool created = false;
                    /*bool isResult = false;
                    EqualsCommand resultCommand;*/
                    public Command(CalculationChain parent, byte number)
                    {
                        this.chain = parent;
                        this.number = number;
                    }
                    public bool Execute()
                    {
                        if (chain.blocks.Count == 0)
                        {
                            this.created = true;
                            NumericBlock block = new NumericBlock(chain);
                            chain.blocks.Push(block);
                            block.value.Append(this.number);
                            return true;
                        }
                        else if (chain.blocks.Peek() is NumericBlock)
                        {
                            ((NumericBlock)chain.blocks.Peek()).value.Append(this.number);
                            return false;
                        }
                        else if(chain.blocks.Peek() is OperationBlock)
                        {
                            this.created = true;
                            NumericBlock block = new NumericBlock(chain);
                            chain.blocks.Push(block);
                            block.value.Append(this.number);
                            return true;
                        }
                        return false;
                    }

                    public void UnExecute()
                    {
                        if (this.created)
                        {
                            chain.blocks.Pop();
                        }
                        else
                        {
                            NumericBlock block = ((NumericBlock)chain.blocks.Peek());
                            block.value.Remove(block.value.Length - 1, 1);
                        }
                        chain.Display();
                    }
                }
            }

            public class OperationBlock : CalculationBlock
            {
                public OperationBlock(CalculationChain parent) : base(parent)
                {
                }
                public override string DisplayValue { get => ((char)calcOperation).ToString(); }
                public CalcOperation calcOperation { get; private set; }
                public enum CalcOperation
                {
                    ADD = 43, SUB = 45, MLT = 42, DIV = 47
                }

                public class Command : CalculationCommand
                {
                    CalculationChain chain;
                    CalcOperation operation;

                    bool wasFirst = false;

                    

                    bool previousExisted = false;
                    CalcOperation previousOperation;
                    public Command(CalculationChain parent, CalcOperation operation)
                    {
                        this.chain = parent;
                        this.operation = operation;
                    }
                    public bool Execute()
                    {
                        if (chain.blocks.Count == 0)
                        {
                            wasFirst= true;
                            NumericBlock numBlock = new NumericBlock(chain, "0");
                            chain.blocks.Push(numBlock);
                            OperationBlock block = new OperationBlock(chain);
                            block.calcOperation = this.operation;
                            chain.blocks.Push(block);
                            return true;

                        }
                        else if (chain.blocks.Peek() is OperationBlock)
                        {
                            previousExisted= true;
                            OperationBlock block = ((OperationBlock)chain.blocks.Peek());
                            previousOperation = block.calcOperation;
                            block.calcOperation = this.operation;
                            return true;
                        }
                        else if (chain.blocks.Peek() is NumericBlock)
                        {
                            OperationBlock block = new OperationBlock(chain);
                            block.calcOperation = this.operation;
                            chain.blocks.Push(block);
                            return true;
                        }
                        return false;
                    }

                    public void UnExecute()
                    {
                        if (this.wasFirst)
                        {
                            chain.blocks.Pop();
                            chain.blocks.Pop();
                        }
                        else if (previousExisted)
                        {
                            OperationBlock block = ((OperationBlock)chain.blocks.Peek());
                            block.calcOperation = previousOperation;
                        }
                        else
                        {
                            chain.blocks.Pop();
                        }
                        chain.Display();
                    }
                }
            }

            public class BracketBlock : CalculationBlock
            {
                public override string DisplayValue { get => ""; }
                public BracketBlock(CalculationChain parent) : base(parent)
                {
                }
            }
        }
    }
}
