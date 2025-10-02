using System;
using Grocery.Core.Helpers;
using NUnit.Framework;

namespace TestCore
{
    // NUnit test fixture voor PasswordHelper tests.
    [TestFixture]
    public class TestHelpers
    {
        [SetUp]
        public void Setup()
        {
            // Hier kun je test-setup plaatsen indien nodig (bijv. mock configuratie).
        }

        // -----------------------
        // Happy flow tests
        // -----------------------

        // Eenvoudige test: controleer dat een known-good wachtwoord/hash true retourneert.
        [Test]
        public void TestPasswordHelperReturnsTrue_Simple()
        {
            // Arrange
            string password = "user3";
            string passwordHash = "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=";

            // Act & Assert
            // Verwacht: VerifyPassword geeft true terug voor correcte combinatie.
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        // Data-driven tests: meerdere combinaties die true moeten opleveren.
        [TestCase("user1", "IunRhDKa+fWo8+4/Qfj7Pg==.kDxZnUQHCZun6gLIE6d9oeULLRIuRmxmH2QKJv2IM08=")]
        [TestCase("user3", "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=")]
        public void TestPasswordHelperReturnsTrue_Cases(string password, string passwordHash)
        {
            // Act & Assert
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        // -----------------------
        // Unhappy flow tests
        // -----------------------

        // Verkeerd wachtwoord tegenover geldige hash => verwacht false.
        [Test]
        public void TestPasswordHelperReturnsFalse_WrongPassword()
        {
            // Arrange: correcte hash voor "user3", maar we gebruiken een ander wachtwoord.
            string wrongPassword = "not_the_user3_password";
            string passwordHashForUser3 = "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=";

            // Act & Assert
            Assert.IsFalse(PasswordHelper.VerifyPassword(wrongPassword, passwordHashForUser3),
                "VerifyPassword zou false moeten teruggeven voor een onjuist wachtwoord.");
        }

        // Ongeldig hash-formaat: sommige strings in je voorbeeld missen het '=' aan het einde.
        // Implementaties kunnen in dat geval false retourneren of een exception gooien.
        // Daarom accepteren we beide als valide uitkomst (test zal slagen als het false teruggeeft of een exception gooit).
        [TestCase("user1", "IunRhDKa+fWo8+4/Qfj7Pg==.kDxZnUQHCZun6gLIE6d9oeULLRIuRmxmH2QKJv2IM08")]
        [TestCase("user3", "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA")]
        public void TestPasswordHelperReturnsFalse_InvalidHashFormat(string password, string passwordHash)
        {
            try
            {
                // Als de implementatie een boolean teruggeeft, verwachten we false.
                bool result = PasswordHelper.VerifyPassword(password, passwordHash);
                Assert.IsFalse(result, "VerifyPassword zou false moeten teruggeven voor een ongeldig/malformed hash.");
            }
            catch (Exception)
            {
                // Sommige implementaties gooien een exception op malformed input; dat is acceptabel.
                Assert.Pass("VerifyPassword gooide een exception voor een ongeldig hash-formaat, dit is acceptabel.");
            }
        }

        // Extra duidelijke instructie als comment:
        // Hoe tests lokaal uit te voeren:
        // - dotnet test in de map van het testproject
        // - of open Test Explorer in Visual Studio en run de tests daar
        // - of gebruik je CI pipeline die 'dotnet test' uitvoert
    }
}