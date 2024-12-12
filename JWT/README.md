
### Implementação de **Token** e **Refresh Token**
Vou me ater principalmente ao processo relacionado a lógica do **Refresh Token**   
O **Refresh Token** é um mecanismo que permite ao cliente obter um novo **Access Token** (JWT) após sua expiração, sem precisar solicitar novamente as credenciais.  
Esse recurso é amplamente utilizado para a segurança de sistemas que utilizam autenticação baseada em tokens.

---

### Processo do Refresh Token

#### 1. **Geração do Access Token e Refresh Token**
   - Quando o usuário realiza a autenticação (credenciais válidas), o servidor gera dois tokens:
     - **Access Token**: Token JWT com informações do usuário, assinado digitalmente, e com um tempo de vida curto (geralmente de 15 minutos a 1 hora).
     - **Refresh Token**: Um token aleatório e seguro, com validade mais longa (geralmente dias ou semanas).
   - O **Access Token** é retornado ao cliente para ser usado em cada requisição.
   - O **Refresh Token** é retornado ao cliente e armazenado, mas não deve ser enviado em requisições normais.

#### 2. **Uso do Access Token**
   - O cliente usa o **Access Token** para acessar recursos protegidos na API.
   - O servidor valida o token verificando:
     - Assinatura.
     - Expiração.
     - Informações (claims) contidas no token.

#### 3. **Quando o Access Token Expira**
   - Caso o **Access Token** expire (retornando um erro `401 Unauthorized`), o cliente verifica se possui um **Refresh Token** válido.
   - O cliente envia o **Refresh Token** para o servidor no endpoint de **refresh**.

#### 4. **Validação do Refresh Token**
   - O servidor verifica a validade do **Refresh Token**:
     - Se ele existe e corresponde ao usuário.
     - Se não está expirado.
     - Se não foi revogado.
   - Caso válido, o servidor gera um novo par de tokens (**Access Token** e **Refresh Token**) e retorna ao cliente.

#### 5. **Revogação do Refresh Token**
   - O **Refresh Token** pode ser revogado pelo servidor:
     - Quando o usuário faz logout.
     - Quando um novo refresh token é emitido, para evitar reutilização do anterior.
   - Essa revogação é feita ao remover o token da base de dados ou cache (Redis, por exemplo).

---
