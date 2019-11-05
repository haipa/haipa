﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Haipa.IdentityDb.Dtos
{
   public class ClientEntityDTO
    {
        public Guid ClientId { get; set; }
        public string Description { get; set; }
        public string SecretKey { get; set; }
        public string[] AllowedScopes { get; set; }
        public bool saveAsFile { get; set; } = false;
    }
}