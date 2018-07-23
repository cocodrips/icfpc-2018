#include<iostream>
#include<string>
#include<sstream>
#include<cmath>
#include<map>

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

map<string,int> commandMap;

void commandMap_init(){
  commandMap["Halt"] = 0;
  commandMap["Wait"] = 1;
  commandMap["Flip"] = 2;
  commandMap["SMove"] = 10;
  commandMap["LMove"] = 20;
  commandMap["FusionP"] = 11;
  commandMap["FusionS"] = 12;
  commandMap["Fission"] = 13;
  commandMap["Fill"] = 14;
  commandMap["Void"] = 15;
  commandMap["GFill"] = 21;
  commandMap["GVoid"] = 22;
}

struct Vec{
  int x,y,z;
};

int Bnum;
struct Bot{
  int id;
  Vec p;
  Vec range;
  int next_action;
  Vec next_arg;
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

string distance_string_vec(Vec t){
  string ret = "<"+to_string(t.x)+","+to_string(t.y)+","+to_string(t.z)+">";
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

Vec next_one_step(Vec f, Vec t){

  int dx = (f.x < t.x) ? 1 : -1;
  int dy = (f.y < t.y) ? 1 : -1;
  int dz = (f.z < t.z) ? 1 : -1;
  /*for(int i=0; i<abs(t.x-f.x);i++){
    cout<<"SMove "<<distance_string(dx,0,0)<<endl;
  }*/

  for(int i=0; i<abs(t.x-f.x);){
    int diff = min(15,(abs(t.x-f.x)-i));
    Vec ret = {dx*diff,0,0};
    return ret;
  }
  /*for(int i=0;i<abs(t.y-f.y);i++){
    cout<<"SMove "<<distance_string(0,dy,0)<<endl;
  }*/
  for(int i=0; i<abs(t.y-f.y);){
    int diff = min(15,(abs(t.y-f.y)-i));
    Vec ret = {0,dy*diff,0};
    return ret;
  }
  /*for(int i=0;i<abs(t.z-f.z);i++){
    cout<<"SMove "<<distance_string(0,0,dz)<<endl;
  }*/
  for(int i=0; i<abs(t.z-f.z);){
    int diff = min(15,(abs(t.z-f.z)-i));
    Vec ret = {0,0,dz*diff};
    return ret;
  }
  Vec ret = {-30,-1,-1};
  return ret;
}

bool inWorld(Vec t){
  if(t.x < 0) return false;
  if(t.x >= R) return false;
  if(t.z < 0) return false;
  if(t.z >= R) return false;
  return true;
}

bool inWorld_range(Vec t, Vec range){
  if(t.x < range.x) return false;
  if(t.x >= range.z) return false;
  if(t.z < 0) return false;
  if(t.z >= R) return false;
  return true;
}

bool isGround(Vec t){
  if(t.y == -1) return true;
  return false;
}

int ddx[5] = {0,1,0,-1,0};
int ddy[5] = {-1,-1,-1,-1,-1};
int ddz[5] = {1,0,-1,0,0};

bool isFill(Vec c){
  if(!inWorld(c)) return false;
  if(Resolutions[c.x][c.y][c.z] == 0)return false;
  if(World[c.x][c.y][c.z]) return false;
  return true;
}

bool isFill_range(Vec c,Vec range){
  if(!inWorld_range(c,range)) return false;
  if(Resolutions[c.x][c.y][c.z] == 0)return false;
  if(World[c.x][c.y][c.z]) return false;
  return true;
}

bool isFill_all(Vec c){
  bool ret = false;
  for(int i=0;i<5;i++){
    Vec tmp = {c.x+ddx[i],c.y+ddy[i],c.z+ddz[i]};
    if(isFill(tmp)) ret = true;
  }
  return ret;
}

bool isFill_all_range(Vec c,Vec range){
  bool ret = false;
  for(int i=0;i<5;i++){
    Vec tmp = {c.x+ddx[i],c.y+ddy[i],c.z+ddz[i]};
    if(isFill_range(tmp,range)) ret = true;
  }
  return ret;
}

Vec isFillArg(int k){
  Vec c = B[k].p;
  for(int i=0;i<5;i++){
    Vec tmp = {c.x+ddx[i],c.y+ddy[i],c.z+ddz[i]};
    if(isFill_range(tmp,B[k].range)){
      return tmp;
    }
  }
  return c;
}

Vec next(Vec c){
  Vec ret = {-1,c.y,-1};
  for(int i=c.x;i<R;i+=2){
    ret.x = i;
    if(i%2){
      int j = (i == c.x) ? c.z+1 : 0;
      for(;j<R;j++){
        ret.z = j;
        if(isFill_all(ret)){
          return ret;
        }
      }
    } else {
      int j = (i == c.x) ? c.z-1 : R-1;
      for(;j>=0;j--){
        ret.z = j;
        if(isFill_all(ret)){
          return ret;
        }
      }
    }
  }
  Vec notfound = {-1,-1,-1};
  return notfound;
}

Vec next_range(Vec c, Vec range){
  Vec ret = {-1,c.y,-1};
  //for(int i=c.x;i<range.z;i++){
  for(int i=range.x;i<range.z;i++){
    ret.x = i;
    //int j = (i == c.x) ? c.z : 0;
    int j= 0;
    for(;j<R;j++){
      ret.z = j;
      if(isFill_all_range(ret, range)){
        return ret;
      }
    }
  }
  Vec notfound = {-1,-1,-1};
  return notfound;
}

void check_ground(Vec c){
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
  return;
}

void grounded_check(){
  for(int i=0;i<Bnum;i++)if(B[i].next_action == 2){
    union_add_one(union_number(B[i].next_arg));
    World[B[i].next_arg.x][B[i].next_arg.y][B[i].next_arg.z] = 1;
    check_ground(B[i].next_arg);
  }
}

void do_flip(){
  cout<<"Flip"<<endl;
  for(int i=1;i<Bnum;i++){
    cout<<"Wait"<<endl;
  }
  return;
}

void output_action(){
  for(int i=0;i<Bnum;i++){
    if(B[i].next_action == -1){
      cout<<"Wait"<<endl;
    }
    if(B[i].next_action == 1){
      cout<<"SMove "<<distance_string_vec(B[i].next_arg)<<endl;
      B[i].p.x += B[i].next_arg.x;
      B[i].p.y += B[i].next_arg.y;
      B[i].p.z += B[i].next_arg.z;
    }
    if(B[i].next_action == 2){
      Vec ag = {(B[i].next_arg.x - B[i].p.x),(B[i].next_arg.y - B[i].p.y),(B[i].next_arg.z - B[i].p.z)};
      cout<<"Fill "<<distance_string_vec(ag)<<endl;
    }
  }
}

void do_action(){
  grounded_check();
  if((union_set_number > 1) && (Harmonics == false)){
    Harmonics ^= true;
    do_flip();
  }
  output_action();
  if((union_set_number == 1) && (Harmonics == true)){
    Harmonics ^= true;
    do_flip();
  }
  return;
}

bool vec_equal(Vec a, Vec b){
  if(a.x != b.x) return false;
  if(a.y != b.y) return false;
  if(a.z != b.z) return false;
  return true;
}

void next_bot_action(int i){
  Vec tmp = B[i].p;
  Vec c = next_range(B[i].p,B[i].range);
  if(c.x == -1){
    B[i].next_action = -1;
    return;
  }
  if(!vec_equal(c,B[i].p)){
    B[i].next_action = 1;
    B[i].next_arg = next_one_step(B[i].p,c);
    return;
  }
  B[i].next_action = 2;
  B[i].next_arg = isFillArg(i);
  return;
}

void sweep_one_plane(int y){ // y-axis plane is to be Fill -> y+1 is bot-plane
  while(1){
  //for(int j=0;j<1;j++){
    bool end = true;
    for(int i=0;i<Bnum;i++){
      next_bot_action(i);
      if(B[i].next_action != -1) end = false;
      //cout<<B[i].next_action<<" ";
      //printVec(B[i].next_arg);
      //cout<<endl;
    }
    if(end){
      //cout<<"break"<<endl;
      break;
    }
    do_action();
  }
  return;
}

void one_step_up(){
  for(int i=0;i<Bnum;i++){
    cout<<"SMove "<<distance_string(0,1,0)<<endl;
    B[i].p.y++;
  }
  return;
}

void update_weight(int y){
  //cout<<"     Update Weight"<<endl;
  int total = 0;
  int weigth[251] = {0};
  for(int i=0;i<R;i++){
    for(int j=0;j<R;j++){
      if(Resolutions[i][y][j]){
        total += 6;
        weigth[i] += 6;
      }
    }
  }

  int point[50];

  int di = total/Bnum;
  int xx = 0;
  for(int i=0;i<Bnum;i++){
    point[i] = xx;
    int tmp = 0;
    while(tmp < di){
      if(xx == R){
        break;
      }
      tmp += weigth[xx++];
    }
  }

  //cout<<"DEBUG";
  //for(int i=0;i<Bnum;i++)cout<<point[i]<<" ";cout<<endl;

  for(int i=0;i<Bnum-1;i++){
    B[i].range.x = min(point[i], R-Bnum+i);
    B[i].range.x = max(B[i].range.x, i);
    B[i].range.z = min(point[i+1], R-Bnum+i+1);
    B[i].range.z = max(B[i].range.z, i+1);
  }
  B[Bnum-1].range.x = min(point[Bnum-1], R-1);
  B[Bnum-1].range.x = max(B[Bnum-1].range.x , Bnum-1);
  B[Bnum-1].range.z = R;

  /*for(int i=0;i<Bnum;i++){
    cout<<i<<" DEBUG "<<B[i].range.x<<" "<<B[i].range.z<<endl;
  }*/

  return;
}

bool inRange(int i){
  if(B[i].p.x < B[i].range.x) return false;
  if(B[i].p.x >= B[i].range.z) return false;
  return true;
}

bool inRange_all(){
  for(int i=0;i<Bnum;i++){
    if(inRange(i) == false) return false;
  }
  return true;
}

void move_acc(){
  Vec next[50];
  for(int i=0;i<Bnum;i++){
    next[i] = B[i].p;
    if(!inRange(i)){
      int dx = (B[i].p.x < B[i].range.x) ? 1 : -1;
      int dist = (B[i].p.x < B[i].range.x) ? abs(B[i].range.x - B[i].p.x) : abs(B[i].range.z - B[i].p.x)+1;
      int ox = abs(B[i+dx].p.x - B[i].p.x)-1;
      dist = min(ox,dist);
      //cout<<"dist "<<dist<<" "<<i<<endl;
      Vec target = {B[i].p.x + (dx*dist) ,B[i].p.y,B[i].p.z};
      Vec step = next_one_step(B[i].p,target);
      if(step.x == -30){
        //printVec(B[i].p);
        //printVec(B[i].range);
        //printVec(target);
        //printVec(step);
        cout<<"Wait"<<endl;
      } else {
        cout<<"SMove "<<distance_string_vec(step)<<endl;
        next[i].x += step.x;
        next[i].y += step.y;
        next[i].z += step.z;
      }
    } else {
      cout<<"Wait"<<endl;
    }
  }
  for(int i=0;i<Bnum;i++){
    B[i].p = next[i];
  }
  return;
}

void move_area(){
  int cnt=0;
  while(1){
    //cout<<"DEBUG "<<i<<endl;
    if(inRange_all())break;
  move_acc();
  return;
}

void search(){
  for(int i=0;i<Rmax;i++){
    update_weight(i);
    one_step_up();
    move_area();
    sweep_one_plane(i);
  }
  return;
}

void output_fission(int i){
  cout<<"Fission <0,0,1> 0"<<endl;
  for(int j=1;j<i;j++){
    cout<<"Wait"<<endl;
  }
}

void output_first_step(int i){
  Vec c = {0,0,1};
  while(1){
    Vec tmp = next_one_step(c,B[i].p);
    //printVec(B[i].p);
    //printVec(tmp);
    if(tmp.x == -30) break;
    for(int j=0;j<i;j++){
      cout<<"Wait"<<endl;
    }
    c.x += tmp.x;
    c.y += tmp.y;
    c.z += tmp.z;
    cout<<"Smove "<<distance_string_vec(tmp)<<endl;
  }
  return;
}

void output_start_posi(int i){
  output_fission(i);
  output_first_step(i);
}

// search function
void set_start_position(){
  for(int i=1;i<Bnum;i++){
    output_start_posi(i);
  }
  return;
}

void return_top(){
  Vec orig = {0,0,0};
  while(1){
    Vec v = next_one_step(B[0].p,orig);
    if(v.x == -30) break;
    cout<<"SMove "<<distance_string_vec(v)<<endl;
    B[0].p.x += v.x;
    B[0].p.y += v.y;
    B[0].p.z += v.z;
    for(int i=1;i<Bnum;i++) cout<<"Wait"<<endl;
  }
}

void output_fussion(int k){
  cout<<"FusionP <0,1,0>"<<endl;
  cout<<"FusionS <0,-1,0>"<<endl;
  for(int i=k+1;i<Bnum;i++) cout<<"Wait"<<endl;
}

void return_i(int k){
  Vec orig = {0,1,0};
  while(1){
    Vec v = next_one_step(B[k].p,orig);
    if(v.x == -30) break;
    cout<<"Wait"<<endl;
    cout<<"SMove "<<distance_string_vec(v)<<endl;
    B[k].p.x += v.x;
    B[k].p.y += v.y;
    B[k].p.z += v.z;
    for(int i=k+1;i<Bnum;i++) cout<<"Wait"<<endl;
  }
  output_fussion(k);
}

void return_end_position(){
  if(Harmonics) do_flip();
  return_top();
  for(int i=1;i<Bnum;i++){
    return_i(i);
  }
  cout<<"Halt"<<endl;
  return;
}

void init_bot(int range){
  for(int i=0;i<(R/range);i++){
    Bnum++;
    Vec v = {i*range,0,0};
    B[i].p = v;
    Vec ran = {i*range,0,i*range+range};
    B[i].range = ran;
  }
  return;
}

int calclen(){
  for(int i=1;i<=R;i++){
    if(R%i == 0){
      int tmp = R/i;
      if(tmp <= 40) return i;
    }
  }
  return R;
}

int main(){
  input_model();
  union_init(R*R*R);
  commandMap_init();
  init_bot(calclen());
  set_start_position();
  search();
  return_end_position();
}
