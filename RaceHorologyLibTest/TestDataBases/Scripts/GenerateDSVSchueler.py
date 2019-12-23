import random

def WriteFile():
  
  fDSV = open("dsvpunkte.txt", "w") 
  fNennliste = open("nennliste.txt", "w") 

  fNennliste.write("Idnr;Stnr;DSV-ID;Name;Vorname;Kateg;JG;V/G;Verein;LPkte\n")


  for lp in range(100):
    id = lp + 1
    dsvid = 10000+id
    name = "NAME%d" % (id)
    firstname = "Vorname%d" %(id)
    year = 2000+(lp%10)
    verein = "Verein %d" % ((id+5)%10)
    verband = "BSV-MU"
    points = random.uniform(1, 9999.99)
    if random.randrange(0,1) == 0:
      sex = "M"
      sex2 = "M"
    else:
      sex = "F"
      sex2 = "W"

    # DSV Punkte Datei
    fDSV.write('{:<10d}{:<20}{:<14}{:<10}{:<30}{:<12}{:7.2f}{:>3}\n'.format(dsvid, name, firstname, year, verein, verband, points, sex))

    fNennliste.write("{};{};{};{};{};{};{};GER;{};{:0.2f}\n".format(id,'',dsvid,name,firstname,sex2,year,verein,points))

  fDSV.write("1000      LSV-PKT.LISTE       13.09.2019    ")

  fDSV.close() 
  fNennliste.close() 



if __name__ == "__main__":
  WriteFile()









