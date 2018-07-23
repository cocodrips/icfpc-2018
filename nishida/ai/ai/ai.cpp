#include<iostream>
#include<string>

using namespace std;

struct Bot{
  int id;
  int x;
  int y;
  int z;
  int subset[20];
};

int Fillcount;
int Resolutions[500][500][500];
int World[500][500][500];
int r;
bool harmonics = false;
string s;
int bnum;
Bot b[20];

void input_model(){
  cin>>r;
  cin>>s;
  for(int i=0;i<(int)s.length();i++){
    if(s[i] == '1'){
      int tmp = i;
      int x = tmp/(r*r);
      int y = (tmp%(r*r))/r;
      int z = (tmp%r);
      Resolutions[x][y][z] = 1;
      Fillcount++;
    }
  }
}

void move_basepoint(int now){
  bool find  = false;
  for(int i=0;i<r;i++){
    for(int j=0;j<r;j++){
      if(Resolutions[i][now][j]){
        //cout<<i<<j<<endl;
        for(int k=0;k<i;k++)cout<<"SMove <1,0,0>"<<endl;
        for(int k=0;k<j;k++)cout<<"SMove <0,0,1>"<<endl;
        b[0].x = i;
        b[0].z = j;
        find = true;
      }
      if(find) break;
    }
    if(find) break;
  }
}

void return_base(){
  cout<<"Flip"<<endl;
  for(int i=0;i<b[0].x;i++)cout<<"SMove <-1,0,0>"<<endl;
  for(int i=0;i<b[0].z;i++)cout<<"SMove <0,0,-1>"<<endl;
  for(int i=0;i<b[0].y;i++)cout<<"SMove <0,-1,0>"<<endl;
  cout<<"Halt"<<endl;
}

void move(int x,int z){
  for(int i=0;i<abs(b[0].x - x);i++){
    if(b[0].x < x) cout<<"SMove <1,0,0>"<<endl;
    else cout<<"SMove <-1,0,0>"<<endl;
  }
  for(int i=0;i<abs(b[0].z - z);i++){
    if(b[0].z < z) cout<<"SMove <0,0,1>"<<endl;
    else cout<<"SMove <0,0,-1>"<<endl;
  }
  b[0].x = x;
  b[0].z = z;
}

void solve(int now){
  for(int i=0;i<r;i++){
    for(int j=0;j<r;j++){
      int tmp =0;
      if(i%2)tmp = r-1-j;
      else tmp = j;
      if(Resolutions[i][now][tmp]){
        move(i,tmp);
        cout<<"Fill <0,-1,0>"<<endl;
      }
    }
  }
}

void output(){
  cout<<"Flip"<<endl;
  for(int i=0;i<r-1;i++){
    cout<<"SMove <0,1,0>"<<endl;
    b[0].y++;
    solve(i);
  }
  return_base();
}

int main(){
  input_model();
  bnum++;
  b[0].id = 0;
  b[0].x = 0;
  b[0].y = 0;
  b[0].z = 0;
  for(int i=1;i<20;i++)b[0].subset[i] = 1;
  output();
}
