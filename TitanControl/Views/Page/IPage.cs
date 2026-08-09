using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Views.Page
{
    public interface IPage
    {
        public PageId Id { get; }
        public void OnShow();
        public void OnHide();
        public void OnRegister();
    }
}
