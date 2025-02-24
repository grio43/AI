/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using SharedComponents.Socks5.Socks;

namespace SharedComponents.Socks5.Plugin
{
    public enum LoginStatus
    {
        Denied = 0xFF,
        Correct = 0x00
    }

    public abstract class LoginHandler : GenericPlugin
    {
        #region Properties

        public abstract bool Enabled { get; set; }

        #endregion Properties

        #region Methods

        public abstract LoginStatus HandleLogin(User user);

        #endregion Methods

        //
    }
}