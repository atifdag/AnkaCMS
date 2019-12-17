import { NgModule } from '@angular/core';
import { HomeIndexComponent } from './home-index/home-index.component';
import { SharedModule } from '../shared/shared.module';



@NgModule({
  declarations: [HomeIndexComponent],
  imports: [
    SharedModule
  ]
})
export class HomeModule { }
