﻿namespace YouLearn.Domain.Args.Usuario
{
    public class AdicionarUsuarioRequest
    {
        public string PrimeiroNome { get; set; }
        public string SegundoNome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}
