﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using NodeCalculator.ViewModels.Nodes;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using NodeCalculator.Models;

namespace NodeCalculator.ViewModels
{
    class NodeFixedConnectionViewModel
    {
        public ReactiveProperty<double> LineFromX { get; }
        public ReactiveProperty<double> LineFromY { get; }

        public ReactiveProperty<double> LineToX { get; }
        public ReactiveProperty<double> LineToY { get; }

        public ReactiveProperty<Visibility> Visible { get; }

        public SolidColorBrush Color { get; } = new SolidColorBrush(Colors.DarkGray);

        protected NodeViewModel Parent;
        private List<IDisposable> disposables = new List<IDisposable>();

        public NodeConnectModel InnerModel { get; private set; }

        public NodeFixedConnectionViewModel(NodeViewModel Node, NodeConnectModel nodeConnectModel)
        {
            Parent = Node;
            InnerModel = nodeConnectModel;

            LineFromX = new ReactiveProperty<double>(0);
            LineFromY = new ReactiveProperty<double>(0);
            LineToX = new ReactiveProperty<double>(0);
            LineToY = new ReactiveProperty<double>(0);
            Visible = new ReactiveProperty<Visibility>(Visibility.Hidden);

            double oneAreaWidth = Node.Width.Value / Node.Out.Count;
            Node.PositionX.Subscribe(x => LineFromX.Value = oneAreaWidth * InnerModel.ConnectIndex + oneAreaWidth / 2 + x);
            Node.PositionY.Subscribe(x => LineFromY.Value = Node.Height.Value + x - 5);

            Node.Out[InnerModel.ConnectIndex].ConnectNode.Subscribe(x => 
            {
                if (x == null)
                {
                    Visible.Value = Visibility.Hidden;

                    foreach(var disposable in disposables)
                    {
                        disposable.Dispose();
                    }
                }
                else
                {
                    Visible.Value = Visibility.Visible;

                    var connectNode = x.Parent;
                    double oneAreaWidth = connectNode.Width.Value / connectNode.In.Count;
                    connectNode.PositionX.Subscribe(pos => LineToX.Value = oneAreaWidth * x.InnerModel.ConnectIndex + oneAreaWidth / 2 + pos).AddTo(disposables);
                    connectNode.PositionY.Subscribe(pos => LineToY.Value = 5 + pos).AddTo(disposables);

                    Node.InnerModel.NextNodes[InnerModel.ConnectIndex].ConnectNode = x.Parent.InnerModel;
                    x.Parent.InnerModel.PrevNodes[x.InnerModel.ConnectIndex].ConnectNode = Node.InnerModel;
                    x.Parent.InnerModel.Do(null);
                }

               
            });
        }
    }
}
