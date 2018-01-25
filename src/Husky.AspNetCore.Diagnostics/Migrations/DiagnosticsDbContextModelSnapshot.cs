﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Husky.AspNetCore.Diagnostics;
using System;

namespace Husky.AspNetCore.Diagnostics.Migrations
{
    [DbContext(typeof(DiagnosticsDbContext))]
    partial class DiagnosticsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Husky.AspNetCore.Diagnostics.ExceptionLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Count");

                    b.Property<string>("ExceptionType")
                        .HasMaxLength(200);

                    b.Property<DateTime>("FirstTime");

                    b.Property<string>("HttpMethod")
                        .HasMaxLength(10);

                    b.Property<DateTime>("LastTime");

                    b.Property<string>("Md5Comparison")
                        .HasMaxLength(32);

                    b.Property<string>("Message")
                        .HasMaxLength(1000);

                    b.Property<string>("Source");

                    b.Property<string>("StackTrace");

                    b.Property<string>("Url")
                        .HasMaxLength(2000);

                    b.Property<string>("UserAgent")
                        .HasMaxLength(1000);

                    b.Property<string>("UserName")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.HasIndex("Md5Comparison")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("ExceptionLogs");
                });
#pragma warning restore 612, 618
        }
    }
}
