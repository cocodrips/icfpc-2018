#include<iostream>
#include<string>
#include<sstream>
#include<cmath>

using namespace std;

// model data
int R;
int Rmax;
int Resolutions[251][251][251];
int World[251][251][251];
int Fillcount;
string S;
bool Harmonics = false;

void input_model(){
  cin>>R;
  cin>>S;
  for(int i=0;i<(int)S.length();i++){
    if(S[i] == '1'){
      int tmp = i;
      int x = tmp/(R*R);
      int y = (tmp%(R*R))/R;
      int z = (tmp%R);
      Resolutions[x][y][z] = 1;
      Fillcount++;
      Rmax = max(Rmax, y+1);
    }
  }
}

//Bot struct
struct Vec{
  int x,y,z;
};

int Bnum;
struct Bot{
  int id;
  Vec p;
};
Bot B[50];

// mapunion data
// id 0 : ground
// id 1~ : box
int union_data[251*251*251];
int union_set_number;

int dx[5] = {0,1,0,-1,0};
int dy[5] = {0,0,0,0,-1};
int dz[5] = {1,0,-1,0,0};

void union_init(int n){
  union_set_number = 1;
  union_data[0] = 0;
  for(int i=1;i<=n;i++){
    union_data[i] = -1;
  }
}

int union_root(int x){
  int tmp = x;
  while(x!=union_data[x]){
    x = union_data[x];
  }
  while(tmp != union_data[tmp]){
    tmp = union_data[tmp];
    union_data[tmp] = x;
  }
  return x;
}

bool union_eq(int x,int y){
  return union_root(x) == union_root(y);
}

int union_join(int x, int y){
  if(union_eq(x,y)) return 0;
  if(x > y) {
    int tmp = x;
    x = y;
    y = tmp;
  }
  union_data[union_root(y)] = union_root(x);
  union_set_number--;
  return 0;
}

int union_add_one(int x){
  union_data[x] = x;
  union_set_number++;
  return 0;
}

int union_number(Vec c){
  return c.x*R*R + c.y*R + c.z + 1;
}


//debug function
void printVec(Vec c){
  cout<<"Vector "<<c.x<<" "<<c.y<<" "<<c.z<<endl;
}


// Move function
string distance_string(int x, int y, int z){
  string ret = "<"+to_string(x)+","+to_string(y)+","+to_string(z)+">";
  return ret;
}

void output_move(Vec f, Vec t){
  int dx = (f.x < t.x) ? 1 : -1;
  int dy = (f.y < t.y) ? 1 : -1;
  int dz = (f.z < t.z) ? 1 : -1;
  /*for(int i=0; i<abs(t.x-f.x);i++){
    cout<<"SMove "<<distance_string(dx,0,0)<<endl;
  }*/
  for(int i=0; i<abs(t.x-f.x);){
    int diff = min(15,(abs(t.x-f.x)-i));
    cout<<"SMove "<<distance_string(dx*diff,0,0)<<endl;
    i += diff;
  }
  /*for(int i=0;i<abs(t.y-f.y);i++){
    cout<<"SMove "<<distance_string(0,dy,0)<<endl;
  }*/
  for(int i=0; i<abs(t.y-f.y);){
    int diff = min(15,(abs(t.y-f.y)-i));
    cout<<"SMove "<<distance_string(0,dy*diff,0)<<endl;
    i += diff;
  }
  /*for(int i=0;i<abs(t.z-f.z);i++){
    cout<<"SMove "<<distance_string(0,0,dz)<<endl;
  }*/
  for(int i=0; i<abs(t.z-f.z);){
    int diff = min(15,(abs(t.z-f.z)-i));
    cout<<"SMove "<<distance_string(0,0,dz*diff)<<endl;
    i += diff;
  }
}

// search function
void set_start_position(){
  //cout<<"Flip"<<endl;
  Bnum++;
  Vec v = {0,0,0};
  B[0].p = v;
  return;
}

Vec next(Vec c){
  Vec ret = {-1,-1,-1};
  for(int i=c.x;i<R;i++){
    if(i%2){
      int j = (i == c.x) ? c.z+1 : 0;
      for(;j<R;j++){
        if(Resolutions[i][c.y][j]){
          ret.x = i;
          ret.y = c.y;
          ret.z = j;
          return ret;
        }
      }
    } else {
      int j = (i == c.x) ? c.z-1 : R-1;
      for(;j>=0;j--){
        if(Resolutions[i][c.y][j]){
          ret.x = i;
          ret.y = c.y;
          ret.z = j;
          return ret;
        }
      }
    }
  }
  Vec notfound = {-1,-1,-1};
  return notfound;
}

bool inWorld(Vec t){
  if(t.x < 0) return false;
  if(t.x >= R) return false;
  if(t.z < 0) return false;
  if(t.z >= R) return false;
  return true;
}

bool isGround(Vec t){
  if(t.y == -1) return true;
  return false;
}

void fill_and_check(Vec c){
  //cout<<union_number(c)<<endl;
  union_add_one(union_number(c));
  //printVec(c);
  //cout<<union_set_number<<endl;
  World[c.x][c.y][c.z] = 1;
  for(int i=0;i<5;i++){
    Vec nt = c;
    nt.x += dx[i];
    nt.y += dy[i];
    nt.z += dz[i];
    if(inWorld(nt)){
      if(isGround(nt)){
        union_join(0,union_number(c));
      } else {
        if(World[nt.x][nt.y][nt.z]){
          union_join(union_number(nt),union_number(c));
        }
      }
    }
  }
  //cout<<union_set_number<<" "<<Harmonics<<endl;
  if((union_set_number > 1) && (Harmonics == false)){
    Harmonics ^= true;
    cout<<"Flip"<<endl;
  }
  cout<<"Fill <0,-1,0>"<<endl;
  if((union_set_number == 1) && (Harmonics == true)){
    Harmonics ^= true;
    cout<<"Flip"<<endl;
  }
  return;
}

void sweep_one_plane(int y){
  Vec c = {0,y,0};
  while(1){
    c = next(c);
    if(c.x == -1) break;
    Vec tmp = c;
    tmp.y++;
    output_move(B[0].p,tmp);
    B[0].p = tmp;
    fill_and_check(c);
  }
  return;
}

void search(){
  for(int i=0;i<=Rmax;i++){
    cout<<"SMove "<<distance_string(0,1,0)<<endl;
    B[0].p.y++;
    sweep_one_plane(i);
  }
  return;
}

void return_end_position(){
  if(Harmonics) cout<<"Flip"<<endl;
  Vec orig = {0,0,0};
  output_move(B[0].p,orig);
  cout<<"Halt"<<endl;
  return;
}

int main(){
  input_model();
  union_init(R*R*R);
  set_start_position();
  search();
  return_end_position();
}
